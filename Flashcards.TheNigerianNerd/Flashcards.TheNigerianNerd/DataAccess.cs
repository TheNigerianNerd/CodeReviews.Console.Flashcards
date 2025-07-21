using Dapper;
using Flashcards.TheNigerianNerd.Models;
using Flashcards.TheNigerianNerd.Models.DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Spectre.Console;

namespace Flashcards.TheNigerianNerd;

public class DataAccess
{
    IConfiguration configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build();

    private string ConnectionString;

    public DataAccess()
    {
        ConnectionString = configuration.GetSection("ConnectionStrings")["DefaultConnection"];
    }
    internal void CreateTables()
    {
        try
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                string createStackTableSql =
                    @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Stacks')
                CREATE TABLE Stacks(
                    Id INT IDENTITY(1,1) NOT NULL,
                    Name NVARCHAR(30) NOT NULL UNIQUE,
                    PRIMARY KEY(Id)
                );";
                conn.Execute(createStackTableSql);

                string createFlashcardTableSql =
                    @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Flashcards')
                CREATE TABLE Flashcards(    
                Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                Question NVARCHAR(30) NOT NULL,
                Answer NVARCHAR(30) NOT NULL,
                StackId INT NOT NULL foreign key references Stacks(Id)
                ON DELETE CASCADE
                ON UPDATE CASCADE);";
                conn.Execute(createFlashcardTableSql);

                string createStudySessionTableSql =
                    @"IF NOT EXISTS(SELECT * FROM sys.tables WHERE name = 'StudySessions')
                    CREATE TABLE StudySessions(
                    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    Questions int NOT NULL,
                    Date DateTime NOT NULL,
                    CorrectAnswers int NOT NULL,
                    Percentage AS (CorrectAnswers * 100) / Questions PERSISTED,
                    Time TIME NOT NULL,
                    StackId int NOT NULL
                        FOREIGN KEY
                        REFERENCES Stacks(Id)
                        ON DELETE CASCADE
                        ON UPDATE CASCADE
                    )";
                conn.Execute(createStudySessionTableSql);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"There was an error creating the tables: {ex.Message}");
        }
    }
    internal void InsertStack(Stack stack)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string insertQuery = @"INSERT INTO Stacks (Name) VALUES (@Name)";

                connection.Execute(insertQuery, new { stack.Name });
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteLine($"Error while inserting stack: {ex.Message.ToString()}");
        }
    }

    internal IEnumerable<Stack> GetAllStacks()
    {
        try
        {
            SqlConnection connection = new(ConnectionString);
            connection.Open();

            string selectQuery = "SELECT * FROM Stacks";

            var records = connection.Query<Stack>(selectQuery);
            return records;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"There was a problem retrieving stacks: {ex.Message}");
            return new List<Stack>();
        }
    }
    internal List<FlashcardDTO> GetAllFlashcards()
    {
        try
        {
            using (SqlConnection connection = new(ConnectionString))
            {
                connection.Open();
                string sqlCommand = "SELECT Flashcards.Question, Flashcards.Answer, Stacks.Name AS StackName FROM Flashcards JOIN Stacks ON Stacks.Id = Flashcards.StackId";

                var flashcards = connection.Query<FlashcardDTO>(sqlCommand).ToList();
                return flashcards;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving flashcards {ex.Message}");
            return new List<FlashcardDTO>();
        }
    }
    internal void InsertFlashcard(Flashcard flashcard)
    {
        SqlConnection connection = new(ConnectionString);
        connection.Open();
        try
        {
            string insertQuery = @"INSERT INTO Flashcards VALUES (@Question, @Answer, @StackId)";
            connection.Execute(insertQuery, new
            {
                flashcard.Question,
                flashcard.Answer,
                flashcard.StackId
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"There was a problem inserting the flashcard: {ex.Message}");
        }
    }
    internal void BulkInsertRecords(List<Stack> stacks, List<Flashcard> flashcards)
    {
        SqlTransaction transaction = null;
        try
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                transaction = connection.BeginTransaction();

                connection.Execute("INSERT INTO STACKS (Name) VALUES (@Name)", stacks, transaction: transaction);
                connection.Execute("INSERT INTO Flashcards (Question, Answer, StackId) VALUES (@Question, @Answer, @StackId)", flashcards, transaction: transaction);

                transaction.Commit(); // Commit the transaction if everything is successful
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"There was a problem bulk inserting records: {ex.Message}");

            if (transaction != null)
            {
                transaction.Rollback(); // Rollback the transaction if an exception occurs
            }
        }
    }
    internal void DeleteTables()
    {
        try
        {
            SqlConnection connection = new(ConnectionString);
            connection.Open();

            string sqlDeleteStudySessions = "DROP TABLE StudySessions";
            connection.Execute(sqlDeleteStudySessions);

            string sqlDeleteCommand = "DROP TABLE Flashcards";
            connection.Execute(sqlDeleteCommand);

            string dropStacksTableSql = @"DROP TABLE Stacks";
            connection.Execute(dropStacksTableSql);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"There was a problem deleting tables: {ex.Message}");
        }
    }
    internal void DeleteFlashcard(int id)
    {
        try
        {
            SqlConnection connection = new(ConnectionString);

            var deleteCommand = @"DELETE FROM Flashcards WHERE Id = @Id";

            int rowsAffected = connection.Execute(deleteCommand, new { Id = id });
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteLine($"Cannot delete flashcard with Id: {id}");
        }
    }
    internal void DeleteStack(int id)
    {
        try
        {
            SqlConnection connection = new SqlConnection(ConnectionString);

            var deleteCommand = "DELETE FROM Stacks WHERE Id = @Id";

            int rowsAffected = connection.Execute(deleteCommand, new { Id = id });
        }
        catch (Exception e)
        {
            Console.WriteLine($"There was a problem deleting the stack with ID: {id} \n Message: {e.Message}");
        }
    }
    internal IEnumerable<Flashcard> GetFlashcards(int stackId)
    {
        try
        {
            SqlConnection connection = new(ConnectionString);

            string getFlashcardCommand = "SELECT * FROM Flashcards WHERE StackId = @StackId";

            var flashcards = connection.Query<Flashcard>(getFlashcardCommand, new { StackId = stackId });

            return flashcards;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not retrieve flashcard with stackId: {stackId} \n Message: {ex.Message}");
            return new List<Flashcard>();
        }
    }
    internal void UpdateStack(Stack stack)
    {
        try
        {
            using (SqlConnection connection = new(ConnectionString))
            {

                string updateCommand = "UPDATE Stacks SET Name = @name WHERE Id = @id";

                int rowsAffected = connection.Execute(updateCommand, new { Name = stack.Name, Id = stack.Id });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"There was a problem updating the stack with Name: {stack.Name} \n Message: {ex.Message}");
        }
    }
    internal void UpdateFlashcard(int flashcardId, Dictionary<string, object> propertiesToUpdate)
    {
        using (var connection = new SqlConnection(ConnectionString))
        {
            connection.Open();

            string updateQuery = "UPDATE flashcards SET ";
            var parameters = new DynamicParameters();

            foreach (var kvp in propertiesToUpdate)
            {
                updateQuery += $"{kvp.Key} = @{kvp.Key}, ";
                parameters.Add(kvp.Key, kvp.Value);
            }

            updateQuery = updateQuery.TrimEnd(',', ' ');

            updateQuery += " WHERE Id = @Id";
            parameters.Add("Id", flashcardId);

            connection.Execute(updateQuery, parameters);
        }
    }
    internal void InsertStudySession(StudySession session)
    {
        try
        {
            using (SqlConnection connection = new(ConnectionString))
            {
                connection.Open();

                string insertQuery = @"
        INSERT INTO StudySessions (Questions, CorrectAnswers, StackId, Time, Date) VALUES (@Questions, @CorrectAnswers, @StackId, @Time, @Date)";

                connection.Execute(insertQuery, new { session.Questions, session.CorrectAnswers, session.StackId, session.Time, session.Date });
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"There was a problem with the study session: {ex.Message}");
        }
    }
    internal List<StudySessionDTO> GetStudySessionData()
    {
        try
        {
            using (SqlConnection connection = new(ConnectionString))
            {
                connection.Open();

                string sql = @"
                            SELECT s.Name as StackName,
                            ss.Date,
                            ss.Questions,
                            ss.CorrectAnswers,
                            ss.Percentage,
                            ss.Time
                            FROM StudySessions ss
                            INNER JOIN Stacks s ON ss.StackId = s.Id;
                            ";

                return connection.Query<StudySessionDTO>(sql).ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unable to select study session data.");
            return new List<StudySessionDTO>();
        }
    }
}
