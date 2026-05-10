module LibrarySystem.Utils

open System

let getStringFullName (fullName: Domain.FullName) =
    let first, last = fullName
    first + " " + last

type LogEntry<'T> = {
    Timestamp: DateTime
    Details: 'T
}
