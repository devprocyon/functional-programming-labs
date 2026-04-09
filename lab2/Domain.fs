module LibrarySystem.Domain

open System

[<Measure>] 
type day

type AuthorId = int
type ReaderId = int
type PublisherId = int
type BookId = int
type BookCopyId = int

type FullName = string * string

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
