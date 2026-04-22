module LibrarySystem.Operations

open System
open Domain
open Utils

let printDescription (item: IDescribable) =
    printfn "DESCRIPTION | %s" (item.Describe())

let borrowBook (currentDate: DateTime) (reader: Reader) (copy: BookCopy) : Result<BookCopy, string> =
    match copy.Status with
    | Available ->
        let dueDate = currentDate.AddDays(float LoanDuration)
        let updatedCopy = { copy with Status = Borrowed(reader, dueDate) }
        Ok updatedCopy
    | Borrowed (r, dueDate) ->
        if r.Id = reader.Id then
            Error (sprintf "This book has already been checked out to this reader until %s." (dueDate.ToShortDateString()))
        else
            Error (sprintf "The book is unavailable. Currently read by: %s." (getStringFullName r.Name))
    | Maintenance ->
        Error "The book is currently undergoing restoration."

let returnBook (copy: BookCopy) : Result<BookCopy, string> =
    match copy.Status with
    | Available -> 
        Error "The book is already in the library."
    | Borrowed _ -> 
        Ok { copy with Status = Available }
    | Maintenance -> 
        Error "Cannot return normally, book is in maintenance."

let sendToMaintenance (copy: BookCopy) : Result<BookCopy, string> =
    match copy.Status with
    | Available -> 
        Ok { copy with Status = Maintenance }
    | _ -> 
        Error "Cannot send to maintenance. Copy is not available."
