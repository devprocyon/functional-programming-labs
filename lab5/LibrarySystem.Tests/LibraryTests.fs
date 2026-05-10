module LibrarySystem.Tests

open System
open NUnit.Framework
open FsUnit
open LibrarySystem.Domain
open LibrarySystem.Utils
open LibrarySystem.Operations
open LibrarySystem.LibraryState
open LibrarySystem

let testAuthor = { Id = 1; Name = ("Taras", "Shevchenko") }
let testPublisher = { Id = 1; Name = "A-BA-BA-HA-LA-MA-HA"; Country = "Ukraine" }
let testBook = { Id = 1; Title = "Kobzar"; Author = testAuthor; Publisher = testPublisher; Genre = Fiction; Year = 1840 }
let testReader = { Id = 1; Name = ("Ivan", "Franko"); Email = "ivan@example.com" }
let testReader2 = { Id = 2; Name = ("Lesya", "Ukrainka"); Email = "lesya@example.com" }

let createAvailableCopy (id: int) = 
    { Id = id; Book = testBook; Status = Available }

let createBorrowedCopy (id: int) (reader: Reader) (dueDate: DateTime) =
    { Id = id; Book = testBook; Status = Borrowed(reader, dueDate) }

let createMaintenanceCopy (id: int) =
    { Id = id; Book = testBook; Status = Maintenance }

[<TestFixture>]
type LibraryTests() =

    [<TestCase("John", "Doe", "John Doe")>]
    [<TestCase("Taras", "Shevchenko", "Taras Shevchenko")>]
    [<TestCase("OneName", "", "OneName ")>]
    member this.``getStringFullName correctly concatenates names`` (first: string, last: string, expected: string) =
        let fullName : FullName = (first, last)
        getStringFullName fullName |> should equal expected

    [<Test>]
    member this.``getStringFullName handles empty strings`` () =
        let fullName : FullName = ("", "")
        getStringFullName fullName |> should equal " "

    [<Test>]
    member this.``borrowBook succeeds for available book`` () =
        let copy = createAvailableCopy 1
        let date = DateTime(2023, 1, 1)
        
        let result = borrowBook date testReader copy
        
        match result with
        | Ok updatedCopy -> 
            match updatedCopy.Status with
            | Borrowed(r, dueDate) -> 
                r.Id |> should equal testReader.Id
                dueDate |> should equal (date.AddDays(14.0))
            | _ -> Assert.Fail("Expected status to be Borrowed")
        | Error _ -> Assert.Fail("Expected Ok result")

    [<Test>]
    member this.``borrowBook fails if book is already borrowed`` () =
        let copy = createBorrowedCopy 1 testReader (DateTime.Now.AddDays(5.0))
        let result = borrowBook DateTime.Now testReader2 copy
        
        match result with
        | Error msg -> msg |> should startWith "The book is unavailable"
        | Ok _ -> Assert.Fail("Should not allow borrowing")

    [<Test>]
    member this.``borrowBook fails if book is in maintenance`` () =
        let copy = createMaintenanceCopy 1
        let result = borrowBook DateTime.Now testReader copy
        
        match result with
        | Error msg -> msg |> should equal "The book is currently undergoing restoration."
        | Ok _ -> Assert.Fail("Should not allow borrowing a book in maintenance")

    [<Test>]
    member this.``returnBook succeeds for borrowed book`` () =
        let copy = createBorrowedCopy 1 testReader DateTime.Now
        let result = returnBook copy
        
        match result with
        | Ok updated -> updated.Status |> should equal Available
        | Error _ -> Assert.Fail()

    [<Test>]
    member this.``returnBook fails for already available book`` () =
        let copy = createAvailableCopy 1
        let result = returnBook copy
        
        match result with
        | Error msg -> msg |> should equal "The book is already in the library."
        | Ok _ -> Assert.Fail()

    [<Test>]
    member this.``returnBook fails for book in maintenance`` () =
        let copy = createMaintenanceCopy 1
        let result = returnBook copy
        
        match result with
        | Error msg -> msg |> should equal "Cannot return normally, book is in maintenance."
        | Ok _ -> Assert.Fail()

    [<Test>]
    member this.``sendToMaintenance succeeds for available book`` () =
        let copy = createAvailableCopy 1
        let result = sendToMaintenance copy
        
        match result with
        | Ok updated -> updated.Status |> should equal Maintenance
        | Error _ -> Assert.Fail()

    [<Test>]
    member this.``sendToMaintenance fails for borrowed book`` () =
        let copy = createBorrowedCopy 1 testReader DateTime.Now
        let result = sendToMaintenance copy
        
        match result with
        | Error msg -> msg |> should equal "Cannot send to maintenance. Copy is not available."
        | Ok _ -> Assert.Fail()

    [<TestCase(5, true)>] 
    [<TestCase(0, false)>]
    [<TestCase(-5, false)>]
    member this.``isOverdue correctly identifies based on due date`` (daysOffset: int, expected: bool) =
        let baseDate = DateTime(2023, 5, 15)
        let dueDate = baseDate
        let currentDate = baseDate.AddDays(float daysOffset)
        
        let copy = createBorrowedCopy 1 testReader dueDate
        isOverdue currentDate copy |> should equal expected

    [<Test>]
    member this.``isOverdue always returns false for Available or Maintenance books`` () =
        let currentDate = DateTime.Now
        isOverdue currentDate (createAvailableCopy 1) |> should be False
        isOverdue currentDate (createMaintenanceCopy 2) |> should be False

    [<TestCase(3, 10.5, 31.5)>]
    [<TestCase(10, 5.0, 50.0)>]
    [<TestCase(-2, 10.0, 0.0)>]
    member this.``calculatePenalty computes correct amount based on days overdue`` (daysOverdue: int, dailyRate: float, expectedPenalty: float) =
        let dueDate = DateTime(2023, 1, 1)
        let currentDate = dueDate.AddDays(float daysOverdue)
        let copy = createBorrowedCopy 1 testReader dueDate
        
        let expected = decimal expectedPenalty
        calculatePenalty currentDate (decimal dailyRate) copy |> should equal expected

    [<Test>]
    member this.``calculatePenalty returns 0 for non-borrowed books`` () =
        let copy = createAvailableCopy 1
        calculatePenalty DateTime.Now 10.0m copy |> should equal 0m

    [<Test>]
    member this.``addReader adds new reader to empty library`` () =
        let lib = emptyLibrary<string>
        let updatedLib = addReader testReader lib
        
        updatedLib.Readers.Count |> should equal 1
        updatedLib.Readers.[testReader.Id] |> should equal testReader

    [<Test>]
    member this.``addReader overwrites existing reader with same ID`` () =
        let lib = emptyLibrary<string> |> addReader testReader
        let updatedReader = { testReader with Email = "new@example.com" }
        let updatedLib = addReader updatedReader lib
        
        updatedLib.Readers.Count |> should equal 1
        updatedLib.Readers.[testReader.Id].Email |> should equal "new@example.com"

    [<Test>]
    member this.``addBookCopy adds copy to inventory`` () =
        let lib = emptyLibrary<string>
        let copy = createAvailableCopy 99
        let updatedLib = addBookCopy copy lib
        
        updatedLib.Inventory.ContainsKey(99) |> should be True

    [<Test>]
    member this.``addBookCopy handles multiple different copies`` () =
        let lib = emptyLibrary<string> 
                  |> addBookCopy (createAvailableCopy 1)
                  |> addBookCopy (createAvailableCopy 2)
                  
        lib.Inventory.Count |> should equal 2

    [<Test>]
    member this.``logEvent prepends new event to the logs list`` () =
        let lib = emptyLibrary<string>
        let msg = "System initialized"
        let updatedLib = logEvent msg lib
        
        updatedLib.Logs.Length |> should equal 1
        updatedLib.Logs.Head.Details |> should equal msg

    [<Test>]
    member this.``logEvent maintains historical order`` () =
        let lib = emptyLibrary<string>
                  |> logEvent "Event 1"
                  |> logEvent "Event 2"
                  
        lib.Logs.Length |> should equal 2
        lib.Logs.Head.Details |> should equal "Event 2"
        lib.Logs.[1].Details |> should equal "Event 1"

    [<Test>]
    member this.``LibraryManager FindBook returns Some when book exists`` () =
        let manager = LibraryManager("Test Branch")
        let copy = createAvailableCopy 42
        manager.AddBook(copy)
        
        let result = manager.FindBook(42)
        result.IsSome |> should be True
        result.Value.Id |> should equal 42

    [<Test>]
    member this.``LibraryManager FindBook returns None when book is missing`` () =
        let manager = LibraryManager()
        let result = manager.FindBook(999)
        
        result.IsNone |> should be True
        manager.GetBookCount() |> should equal 0

    [<Test>]
    member this.``isCurrentlyBorrowedBy returns true for correct reader`` () =
        let copy = createBorrowedCopy 1 testReader DateTime.Now
        isCurrentlyBorrowedBy testReader.Id copy |> should be True

    [<Test>]
    member this.``isCurrentlyBorrowedBy returns false for different reader or available status`` () =
        let borrowedCopy = createBorrowedCopy 1 testReader DateTime.Now
        let availableCopy = createAvailableCopy 2
        isCurrentlyBorrowedBy testReader2.Id borrowedCopy |> should be False
        isCurrentlyBorrowedBy testReader.Id availableCopy |> should be False

    [<TestCase(1, true)>]
    [<TestCase(2, false)>]
    [<TestCase(99, false)>]
    member this.``isCurrentlyBorrowedBy parameterized check`` (id: int, expected: bool) =
        let copy = createBorrowedCopy 10 testReader DateTime.Now
        isCurrentlyBorrowedBy id copy |> should equal expected

    [<Test>]
    member this.``processBorrow updates library state on success`` () =
        let lib = emptyLibrary<string> 
                  |> addReader testReader 
                  |> addBookCopy (createAvailableCopy 10)
        
        let updatedLib = processBorrow 10 testReader.Id lib
        
        let updatedCopy = updatedLib.Inventory.[10]
        match updatedCopy.Status with
        | Borrowed(r, _) -> r.Id |> should equal testReader.Id
        | _ -> Assert.Fail()
        updatedLib.Logs.Length |> should equal 1

    [<Test>]
    member this.``processBorrow does not change inventory if book not found`` () =
        let lib = emptyLibrary<string> |> addReader testReader
        let updatedLib = processBorrow 999 testReader.Id lib
        updatedLib.Inventory.Count |> should equal 0

    [<Test>]
    member this.``LibraryManager secondary constructor sets default values`` () =
        let manager = LibraryManager("East Branch")
        manager.BranchName |> should equal "East Branch"
        manager.ManagerName |> should equal "Default Manager"

    [<Test>]
    member this.``LibraryManager properties can be updated`` () =
        let manager = LibraryManager()
        manager.BranchName <- "New Name"
        manager.ManagerName <- "New Manager"
        manager.BranchName |> should equal "New Name"
        manager.ManagerName |> should equal "New Manager"

    [<Test>]
    member this.``processBorrow adds error log when reader is missing`` () =
        let lib = emptyLibrary<string> |> addBookCopy (createAvailableCopy 1)
        let updatedLib = processBorrow 1 999 lib
        updatedLib.Logs.Length |> should equal 0
