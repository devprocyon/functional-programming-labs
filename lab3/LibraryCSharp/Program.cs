using LibrarySystem;
using static LibrarySystem.Domain;

class Program
{
    static void Main()
    {
        var manager = new LibraryManager("Kyiv Tech Library", "Oleksandr");
        Console.WriteLine($"Manager: {manager.ManagerName} at {manager.BranchName}");

        var author = new Author(1, Tuple.Create("Arthur", "Conan Doyle"));
        var pub = new Publisher(1, "George Newnes", "UK");
        
        var genre = Genre.Fiction;

        var book = new Book(2, "Sherlock Holmes", author, pub, genre, 1892);

        var status = BookStatus.Available;
        var copy = new BookCopy(501, book, status);

        manager.AddBook(copy);
        Console.WriteLine($"Total books in manager: {manager.GetBookCount()}");

        var reader = new Reader(1, Tuple.Create("Ivan", "Ivanov"), "ivan@test.com");
        var result = Operations.borrowBook(DateTime.Now, reader, copy);

        if (result.IsOk)
        {
            Console.WriteLine("Book borrowed successfully!");
        }
        else
        {
            Console.WriteLine($"Error - {result.ErrorValue}");
        }
    }
}
