module LibrarySystem.Domain

open System

[<Measure>] 
type day

type IDescribable =
    abstract member Describe : unit -> string

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
with
    interface IDescribable with
        member this.Describe() =
            let (first, last) = this.Name
            sprintf "Author: %s %s (ID: %d)" first last this.Id

type Reader = {
    Id: ReaderId
    Name: FullName
    Email: string
}
with
    interface IDescribable with
        member this.Describe() =
            let (first, last) = this.Name
            sprintf "Reader: %s %s, Contact: %s" first last this.Email

type Publisher = {
    Id: PublisherId
    Name: string
    Country: string
}
with
    interface IDescribable with
        member this.Describe() =
            sprintf "Publisher: %s (%s)" this.Name this.Country

type Book = {
    Id: BookId
    Title: string
    Author: Author
    Publisher: Publisher
    Genre: Genre
    Year: int
}
with
    interface IDescribable with
        member this.Describe() =
            sprintf "Book: %s (%d), Genre: %A" this.Title this.Year this.Genre

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
