using Flashcards.TheNigerianNerd;

var DataAccess = new DataAccess();
DataAccess.CreateTables();
SeedData.SeedRecords();
UserInterface.MainMenu();