module LibrarySystem.Program

open LibrarySystem.Domain
open LibrarySystem.Operations

[<EntryPoint>]
let main argv =
    let author = { Id = 1; Name = ("Robert", "Martin") }
    let pub = { Id = 1; Name = "Prentice Hall"; Country = "USA" }
    let book = { Id = 1; Title = "Clean Code"; Author = author; Publisher = pub; Genre = Programming; Year = 2008 }
    let copy = { Id = 101; Book = book; Status = Available }

    printDescription (book :> IDescribable)

    let customItem = {
        new IDescribable with
            member _.Describe() = "I am a custom temporary item in the library system."
    }
    printDescription customItem

    0
