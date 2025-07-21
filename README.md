# Console Flashcards App (CodeReviews.Console.Flashcards)

A C# console application for creating, storing, and reviewing flashcards, structured according to The CSharp Academy’s best practices.

---

## 🚀 Overview

This app enables users to:

- **Create and manage decks** of flashcards  
- **Add, edit, and delete** flashcards within decks  
- **Review decks** interactively in quiz mode  
- **Track performance**, marking flashcards as correct or incorrect  
- **Persist data** locally (e.g., JSON files) between sessions

It follows a layered architecture—Controllers, DTOs, Models, Views, Enums, Helpers—mirroring the CodeReviews.Console.Flashcards review guidelines.

---

## 📁 Project Structure

CodeReviews.Console.Flashcards/
├── Controllers/ # Orchestrate user input and app logic
├── DTO/ # Data Transfer Objects for serialization/persistence
├── Models/ # Domain entities: Flashcard, Deck, Stats
├── Views/ # Console-based UI & menus
├── Enums/ # Enums for statuses or difficulty
├── Helpers/ # Utility functions: I/O, parsing, formatting
├── Program.cs # Entry point
└── CodeReviews.Console.Flashcards.csproj

---

## 🛠 Getting Started

### Prerequisites

- [.NET 6.0 SDK or higher](https://dotnet.microsoft.com/download)

### Setup Instructions

```bash
git clone https://github.com/TheNigerianNerd/CodeReviews.Console.Flashcards.git
cd CodeReviews.Console.Flashcards
dotnet restore
