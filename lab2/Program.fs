module LibrarySystem.Program

open Domain
open LibraryState

let createAuthor id firstName lastName = { Id = id; Name = (firstName, lastName) }
let createPublisher id name country = { Id = id; Name = name; Country = country }
let registerReader id firstName lastName email = { Id = id; Name = (firstName, lastName); Email = email }
let publishBook id title author publisher genre year = { Id = id; Title = title; Author = author; Publisher = publisher; Genre = genre; Year = year }
let createBookCopy copyId book = { Id = copyId; Book = book; Status = Available }

[<EntryPoint>]
let main argv =
    let author1 = createAuthor 1 "Robert" "Martin"
    let author2 = createAuthor 2 "Arthur" "Conan Doyle"
    
    let pub1 = createPublisher 1 "Prentice Hall" "USA"
    let pub2 = createPublisher 2 "George Newnes" "UK"

    let book1 = publishBook 1 "Clean code" author1 pub1 Programming 2008
    let book2 = publishBook 2 "The Adventures of Sherlock Holmes" author2 pub2 Fiction 1892

    let reader1 = registerReader 1 "Ivan" "Petrenko" "ivan@example.com"
    let reader2 = registerReader 2 "Maria" "Kovalenko" "maria@example.com"

    let copy1 = createBookCopy 1 book1
    let copy2 = createBookCopy 2 book2
    let copy3 = createBookCopy 3 book2

    let initialLibrary = 
        emptyLibrary<string>
        |> addReader reader1
        |> addReader reader2
        |> addBookCopy copy1
        |> addBookCopy copy2
        |> addBookCopy copy3

    printfn "\n--- Operations Simulation ---"
    
    let libAfterBorrow = processBorrow 1 1 initialLibrary
    
    let libAfterFailedBorrow = processBorrow 1 2 libAfterBorrow
    
    let libAfterMissingCopy = processBorrow 99 1 libAfterFailedBorrow

    printAvailableBooksOfGenre Fiction libAfterMissingCopy
    printAvailableBooksOfGenre Programming libAfterMissingCopy

    0
