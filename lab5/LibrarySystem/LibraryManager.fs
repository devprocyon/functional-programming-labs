namespace LibrarySystem

open LibrarySystem.Domain

type LibraryManager(branchName: string, managerName: string) =
    let mutable inventory = Map.empty<BookCopyId, BookCopy>
    
    new(branchName: string) = LibraryManager(branchName, "Default Manager")
    new() = LibraryManager("Central Library", "System Admin")

    member val BranchName = branchName with get, set
    member val ManagerName = managerName with get, set

    member this.AddBook(copy: BookCopy) =
        inventory <- inventory.Add(copy.Id, copy)
        printfn "Book added to %s" this.BranchName

    member this.GetBookCount() = inventory.Count
    
    member this.FindBook(id: int) =
        inventory.TryFind id
