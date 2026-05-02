open System
open System.IO
open FSharp.Data

[<Literal>]
let SampleJson = """[
  {
    "id": 1,
    "title": "Sample",
    "author": "Author",
    "year": 2000,
    "genre": "Genre",
    "pages": 100,
    "isAvailable": true,
    "borrowCount": 10
  }
]"""

type LibraryData = JsonProvider<SampleJson>

let analyzeLibraryDataAsync (filePath: string) =
    async {
        try
            printfn "Asynchronously loading data from %s...\n" filePath
            
            let! books = LibraryData.AsyncLoad(filePath)

            if books.Length = 0 then
                printfn "The file is empty or does not contain a book array."
            else
                let mostPopular = 
                    books |> Array.maxBy (fun b -> b.BorrowCount)
                
                printfn "--- 1. Most Popular Book ---"
                printfn "Title: '%s', Author: %s (Borrows: %d)\n" mostPopular.Title mostPopular.Author mostPopular.BorrowCount

                printfn "--- 2. Average Page Count by Genre ---"
                books
                |> Array.groupBy (fun b -> b.Genre)
                |> Array.map (fun (genre, genreBooks) -> 
                    let avgPages = genreBooks |> Array.averageBy (fun b -> float b.Pages)
                    (genre, avgPages)
                )
                |> Array.iter (fun (genre, avg) -> 
                    printfn "Genre: %-20s | Average Length: %.1f pages" genre avg
                )
                printfn ""

                printfn "--- 3. Retro Books in Stock (Published before 2000) ---"
                let retroAvailableBooks = 
                    books |> Array.filter (fun b -> b.Year < 2000 && b.IsAvailable)
                
                if retroAvailableBooks.Length > 0 then
                    retroAvailableBooks 
                    |> Array.iter (fun b -> printfn " - '%s' (%d year)" b.Title b.Year)
                else
                    printfn "No such books are currently available."
                printfn ""

        with
        | :? FileNotFoundException -> 
            printfn "Error: File '%s' not found." filePath
        | ex -> 
            printfn "An unexpected error occurred: %s" ex.Message
    }

[<EntryPoint>]
let main argv =
    Console.OutputEncoding <- Text.Encoding.UTF8 
    
    let filePath = 
        if argv.Length > 0 then 
            argv.[0] 
        else 
            printf "Enter the path to the JSON file (e.g., library.json): "
            Console.ReadLine()

    if String.IsNullOrWhiteSpace(filePath) then
        printfn "The file path cannot be empty."
        1
    else
        analyzeLibraryDataAsync filePath
        |> Async.RunSynchronously
        
        0
