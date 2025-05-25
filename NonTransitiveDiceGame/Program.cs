using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Error: At least 3 dice must be provided.");
            Console.WriteLine("Example: dotnet run \"2,2,4,4,9,9\" \"6,8,1,1,8,6\" \"7,5,3,7,5,3\"");
            return;
        }

        var diceList = new List<List<int>>();
        for (int i = 0; i < args.Length; i++)
        {
            var values = args[i].Split(',');
            if (values.Length != 6 || !values.All(v => int.TryParse(v, out _)))
            {
                Console.WriteLine($"Error: Dice #{i + 1} is invalid. Use 6 integers like: 2,2,4,4,9,9");
                return;
            }
            diceList.Add(values.Select(int.Parse).ToList());
        }

        Console.WriteLine("🎲 Welcome to the Non-Transitive Dice Game!");
        Console.WriteLine();

        var tossResult = FairCoinToss(out string userSeed, out string computerSeed);
        Console.WriteLine($"Fair toss result: {(tossResult ? "You go first" : "Computer goes first")}");
        Console.WriteLine($"Computer commit hash: {GetSha256Hash(computerSeed)}");
        Console.WriteLine();

        int userChoice = GetUserChoice(diceList);
        if (userChoice == -1) return;

        int computerChoice = GetComputerChoice(diceList.Count, userChoice);
        Console.WriteLine($"Computer chose die #{computerChoice + 1}");

        int userRoll = RollDiceFair(diceList[userChoice], out string userRollSeed);
        int computerRoll = RollDiceFair(diceList[computerChoice], out string computerRollSeed);

        Console.WriteLine($"\nYour roll: {userRoll} (Seed: {userRollSeed})");
        Console.WriteLine($"Computer roll: {computerRoll} (Seed: {computerRollSeed})");

        if (userRoll > computerRoll)
            Console.WriteLine(" You win!");
        else if (userRoll < computerRoll)
            Console.WriteLine(" Computer wins!");
        else
            Console.WriteLine(" It's a tie!");
    }

    static int GetUserChoice(List<List<int>> diceList)
    {
        while (true)
        {
            Console.WriteLine("Choose your die:");
            for (int i = 0; i < diceList.Count; i++)
                Console.WriteLine($"{i + 1}. Die {i + 1}: [{string.Join(",", diceList[i])}]");

            Console.WriteLine("H. Help");
            Console.WriteLine("X. Exit");

            var input = Console.ReadLine();
            if (input?.ToUpper() == "H")
            {
                Console.WriteLine("Enter the number of the die you want to choose (e.g., 1).");
            }
            else if (input?.ToUpper() == "X")
            {
                Console.WriteLine("Goodbye!");
                return -1;
            }
            else if (int.TryParse(input, out int choice) && choice >= 1 && choice <= diceList.Count)
            {
                return choice - 1;
            }
            else
            {
                Console.WriteLine("Invalid input. Try again.");
            }
        }
    }

    static int GetComputerChoice(int total, int userChoice)
    {
        Random rnd = new();
        int choice;
        do
        {
            choice = rnd.Next(total);
        } while (choice == userChoice);
        return choice;
    }

    static int RollDiceFair(List<int> dice, out string seed)
    {
        seed = Guid.NewGuid().ToString();
        int hash = Math.Abs(GetSha256Hash(seed).GetHashCode());
        return dice[hash % dice.Count];
    }

    static bool FairCoinToss(out string userSeed, out string computerSeed)
    {
        userSeed = Guid.NewGuid().ToString();
        computerSeed = Guid.NewGuid().ToString();

        int userBit = GetSha256Hash(userSeed).Last() % 2;
        int compBit = GetSha256Hash(computerSeed).Last() % 2;

        return (userBit ^ compBit) == 1;
    }

    static string GetSha256Hash(string input)
    {
        using SHA256 sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "");
    }
}

