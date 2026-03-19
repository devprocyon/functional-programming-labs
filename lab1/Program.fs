open System

// Units of measurement
[<Measure>] 
type day

// Aliases
type AuthorId = int
type ReaderId = int
type PublisherId = int
type BookId = int
type BookCopyId = int

type FullName = string * string // First name * Last name

type Genre =
    | Fiction
    | Science
    | Programming
    | History

type Author = {
    Id: AuthorId
    Name: FullName
}

type Reader = {
    Id: ReaderId
    Name: FullName
    Email: string
}

type Publisher = {
    Id: PublisherId
    Name: string
    Country: string
}

type Book = {
    Id: BookId
    Title: string
    Author: Author
    Publisher: Publisher
    Genre: Genre
    Year: int
}

type BookStatus =
    | Available
    | Borrowed of Reader * DateTime
    | Maintenance

type BookCopy = {
    Id: BookCopyId
    Book: Book
    Status: BookStatus
}

let LoanDuration: float<day> = 14.0<day>

let getStringFullName (fullName: FullName) =
    let first, last = fullName
    first + " " + last

let createAuthor id firstName lastName : Author =
    { Id = id; Name = firstName, lastName }

let createPublisher id name country : Publisher =
    { Id = id; Name = name; Country = country }

let changeBookStatus newStatus copy =
    { copy with Status = newStatus }

let registerReader id firstName lastName email : Reader =
    { Id = id; Name = firstName, lastName; Email = email }

let publishBook id title author publisher genre year : Book =
    { Id = id; Title = title; Author = author; Publisher = publisher; Genre = genre; Year = year }

let addBookCopy copyId book : BookCopy =
    { Id = copyId; Book = book; Status = Available }

let borrowBook (currentDate: DateTime) (reader: Reader) copy =
    match copy.Status with
    | Available ->
        let dueDate = currentDate.AddDays(float LoanDuration)
        printfn "Book '%s' was successfully checked out to reader %s by %s." copy.Book.Title (getStringFullName reader.Name) (dueDate.ToShortDateString())
        changeBookStatus (Borrowed(reader, dueDate)) copy
    | Borrowed (r, dueDate) ->
        if r.Id = reader.Id then
            printfn "This book has already been checked out to this reader (should be returned to %s)." (dueDate.ToShortDateString())
            copy
        else
            printfn "The book is unavailable. It is currently being read by: %s." (getStringFullName r.Name)
            copy
    | Maintenance ->
        printfn "The book '%s' is currently undergoing restoration." copy.Book.Title
        copy

let returnBook copy =
    match copy.Status with
    | Available ->
        printfn "The book '%s' is already in the library." copy.Book.Title
        copy
    | Borrowed (reader, _) ->
        printfn "Reader %s returned the book '%s'." (getStringFullName reader.Name) copy.Book.Title
        changeBookStatus Available copy
    | Maintenance ->
        copy

let sendToMaintenance copy =
    match copy.Status with
    | Available ->
        printfn "Copy of '%s' sent to maintenance." copy.Book.Title
        changeBookStatus Maintenance copy
    | _ ->
        printfn "Cannot send '%s' to maintenance. Copy is not available." copy.Book.Title
        copy

// Partial application of the function
let borrowBookToday = borrowBook DateTime.Now

let isOfGenre targetGenre copy = copy.Book.Genre = targetGenre

let isAvailable copy =
    match copy.Status with
    | Available -> true
    | _ -> false

let getBookTitle item = item.Book.Title

// Pipeline
let printAvailableBooksOfGenre genre library =
    printfn "\n--- Available books in the genre %A ---" genre
    library
    |> List.filter (isOfGenre genre)
    |> List.filter isAvailable
    |> List.map getBookTitle
    |> List.iter (printfn "- %s")

// Composition
let printBookTitle = getBookTitle >> printfn "Book name: %s"

[<EntryPoint>]
let main args =
    let author1 = createAuthor 1 "Robert" "Martin"
    let author2 = createAuthor 2 "Arthur" "Conan Doyle"

    let pub1 = createPublisher 1 "Prentice Hall" "USA"
    let pub2 = createPublisher 2 "George Newnes" "UK"

    let book1 = publishBook 1 "Clean code" author1 pub1 Programming 2008
    let book2 = publishBook 2 "The Adventures of Sherlock Holmes" author2 pub2 Fiction 1892

    let reader1 = registerReader 1 "Ivan" "Petrenko" "ivan@example.com"
    let reader2 = registerReader 2 "Maria" "Kovalenko" "maria@example.com"

    let copy1 = addBookCopy 1 book1
    let copy2 = addBookCopy 2 book2
    let copy3 = addBookCopy 3 book2
    
    printfn "\n--- Issuance operations ---"
    let borrowedCopy1 = borrowBookToday reader1 copy1
    let failedBorrowCopy1 = borrowBookToday reader2 borrowedCopy1

    printfn "\n--- Maintenance operations ---"
    let maintenanceCopy = sendToMaintenance copy2
    let failedMaintenance = sendToMaintenance borrowedCopy1

    let updatedLibrary = [borrowedCopy1; maintenanceCopy; copy3]

    printAvailableBooksOfGenre Fiction updatedLibrary
    printAvailableBooksOfGenre Programming updatedLibrary

    printfn "\n--- Return operations ---"
    let returnedItem1 = returnBook borrowedCopy1

    printfn "\n--- Composition demonstration ---"
    printBookTitle returnedItem1

    0
