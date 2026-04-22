module LibrarySystem.LibraryState

open System
open Domain
open Operations
open Utils

type Library<'TLog> = {
    Readers: Map<ReaderId, Reader>
    Inventory: Map<BookCopyId, BookCopy>
    Logs: LogEntry<'TLog> list
}

let emptyLibrary<'TLog> : Library<'TLog> = 
    { Readers = Map.empty; Inventory = Map.empty; Logs = [] }

let addReader (reader: Reader) (lib: Library<'TLog>) =
    { lib with Readers = Map.add reader.Id reader lib.Readers }

let addBookCopy (copy: BookCopy) (lib: Library<'TLog>) =
    { lib with Inventory = Map.add copy.Id copy lib.Inventory }

let logEvent<'TLog> (event: 'TLog) (lib: Library<'TLog>) : Library<'TLog> =
    let newEntry = { Timestamp = DateTime.Now; Details = event }
    { lib with Logs = newEntry :: lib.Logs }

let processBorrow (copyId: BookCopyId) (readerId: ReaderId) (lib: Library<string>) : Library<string> =
    let copyOpt = Map.tryFind copyId lib.Inventory
    let readerOpt = Map.tryFind readerId lib.Readers

    match copyOpt, readerOpt with
    | Some copy, Some reader ->
        match borrowBook DateTime.Now reader copy with
        | Ok updatedCopy ->
            let msg = sprintf "Success: Book '%s' borrowed by %s" copy.Book.Title (getStringFullName reader.Name)
            printfn "%s" msg
            lib |> addBookCopy updatedCopy |> logEvent msg
        | Error errMsg ->
            printfn "Failed to borrow: %s" errMsg
            lib |> logEvent (sprintf "Borrow failed for copyId %d: %s" copyId errMsg)
    | None, _ -> 
        printfn "Error: Book copy not found."
        lib
    | _, None -> 
        printfn "Error: Reader not found."
        lib

let printAvailableBooksOfGenre (targetGenre: Genre) (lib: Library<'TLog>) =
    printfn "\n--- Available books in the genre %A ---" targetGenre
    lib.Inventory
    |> Map.toList 
    |> List.map snd
    |> List.filter (fun copy -> copy.Book.Genre = targetGenre)
    |> List.filter (fun copy -> match copy.Status with | Available -> true | _ -> false)
    |> List.iter (fun copy -> printfn "- %s" copy.Book.Title)
