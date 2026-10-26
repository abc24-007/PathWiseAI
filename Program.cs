using PathWiseAI;
using System;
using System.Collections.Generic;
using System.Linq;

// Draft questions and explanations: awaiting educator review.
var quizzes = new Dictionary<string, List<QuizItem>>
{
    ["C# Loops"] = new()
    {
        new(
            "loops1",
            "How many times does this loop run?\n" +
            "for (int i = 0; i < 3; i++)",
            new[] { "2", "3", "4", "0" },
            1,
            "It runs for i = 0, 1 and 2: three iterations."),

        new(
            "loops2",
            "Which loop checks its condition after the body?",
            new[] { "for", "while", "do while", "foreach" },
            2,
            "A do while loop executes its body before checking " +
            "the condition, so it runs at least once."),

        new(
            "loops3",
            "What does break do inside a loop?",
            new[]
            {
                "Exits the loop",
                "Restarts the loop",
                "Increases the counter",
                "Skips only the current iteration"
            },
            0,
            "break exits the nearest enclosing loop."),

        new(
            "loops4",
            "What does continue do inside a loop?",
            new[]
            {
                "Closes the program",
                "Resets the counter",
                "Always ends the loop",
                "Skips the rest of the current iteration"
            },
            3,
            "continue skips the remaining statements in the " +
            "current iteration and proceeds with the loop."),

        new(
            "loops5",
            "What is the final value of sum?\n" +
            "int sum = 0;\n" +
            "for (int i = 1; i <= 3; i++) sum += i;",
            new[] { "3", "5", "6", "9" },
            2,
            "The loop adds 1 + 2 + 3, giving 6.")
    },

    ["IPv4 Basics"] = new()
    {
        new(
            "ip1",
            "How many bits are in an IPv4 address?",
            new[] { "16", "32", "64", "128" },
            1,
            "IPv4 has four octets of eight bits: 32 bits."),

        new(
            "ip2",
            "Which subnet mask represents /24?",
            new[]
            {
                "255.255.255.0",
                "255.255.0.0",
                "255.0.0.0",
                "255.255.255.255"
            },
            0,
            "/24 means 24 network bits, represented by " +
            "255.255.255.0."),

        new(
            "ip3",
            "What is the network address of 192.168.1.25/24?",
            new[]
            {
                "192.168.1.25",
                "192.168.1.255",
                "192.168.0.0",
                "192.168.1.0"
            },
            3,
            "With /24, the first three octets identify the " +
            "network. Setting the host bits to zero gives " +
            "192.168.1.0."),

        new(
            "ip4",
            "What is the broadcast address of 192.168.1.0/24?",
            new[]
            {
                "192.168.1.1",
                "192.168.1.0",
                "192.168.1.255",
                "192.168.0.255"
            },
            2,
            "Setting all eight host bits to one gives " +
            "192.168.1.255."),

        new(
            "ip5",
            "How many conventionally usable host addresses " +
            "are in a /24 subnet?",
            new[] { "256", "254", "255", "24" },
            1,
            "Eight host bits give 256 addresses. Subtract " +
            "the network and broadcast addresses: 254.")
    }
};

var model = new LearnerModel();
var historyStore = new HistoryStore();
List<TopicResult> history;

try
{
    history = historyStore.Load();
}
catch (Exception error) when (
    error is System.IO.IOException ||
    error is UnauthorizedAccessException ||
    error is System.Text.Json.JsonException)
{
    Console.WriteLine(
        "Could not load history. Existing data has been preserved.");
    Console.WriteLine(error.Message);
    Console.WriteLine("Press Enter to close.");
    Console.ReadLine();
    return;
}

Console.WriteLine("PATHWISE AI — INTERACTIVE PROTOTYPE");
Console.WriteLine("Your answers will be used to calculate your score.");
Console.WriteLine("Content and thresholds await educator review.");
Console.WriteLine("Attempt history is saved on this computer.");
Console.WriteLine($"History file: {historyStore.FilePath}");
while (true)
{
    Console.WriteLine("\nMAIN MENU");
    Console.WriteLine("1. C# — Loops");
    Console.WriteLine("2. Networking — IPv4 Basics");
    Console.WriteLine("3. View attempt history");
    Console.WriteLine("4. Exit");

    int choice = ReadChoice(4);
    if (choice == 4) break;

    if (choice == 3)
    {
        if (history.Count == 0)
        {
            Console.WriteLine("No assessments completed yet.");
        }
        else
        {
            for (int i = 0; i < history.Count; i++)
            {
                var attempt = history[i];
                Console.WriteLine(
                    $"Attempt {i + 1}: {attempt.Topic} | " +
                    $"{attempt.Score:0.0}% | {attempt.Level}");
            }
        }

        continue;
    }

    string topic = choice == 1 ? "C# Loops" : "IPv4 Basics";
    var items = quizzes[topic];

    if (history.Any(attempt => attempt.Topic == topic))
    {
        Console.WriteLine(
            "\nThis repeat uses the same questions for practice.");
        Console.WriteLine(
            "A higher score may reflect familiarity with the questions.");
    }

    var questions = items.Select(item =>
        new Question(item.Id, topic, item.CorrectOption)).ToList();

    var answers = new List<Answer>();

    Console.WriteLine($"\nASSESSMENT: {topic}");

    for (int i = 0; i < items.Count; i++)
    {
        var item = items[i];
        Console.WriteLine($"\nQuestion {i + 1}: {item.Prompt}");

        for (int option = 0; option < item.Options.Length; option++)
        {
            Console.WriteLine(
                $"{option + 1}. {item.Options[option]}");
        }

        int selectedOption = ReadChoice(4) - 1;
        answers.Add(new Answer(item.Id, selectedOption));
    }

    var result = model.Assess(questions, answers);
    history.Add(result);
    try
    {
        historyStore.Save(history);
        Console.WriteLine("\nAttempt saved successfully.");
    }
    catch (Exception error) when (
        error is System.IO.IOException ||
        error is UnauthorizedAccessException)
    {
        Console.WriteLine(
            "\nCould not save this attempt. It remains in session memory.");
        Console.WriteLine(error.Message);
    }

    Console.WriteLine("\nYOUR RESULTS");
    Console.WriteLine($"Topic: {result.Topic}");
    Console.WriteLine(
        $"Correct answers: {result.Correct}/{result.Answered}");
    Console.WriteLine($"Score: {result.Score:0.0}%");
    Console.WriteLine($"Provisional classification: {result.Level}");

    Console.WriteLine("\nSUGGESTED NEXT ACTION");
    Console.WriteLine(model.GetSuggestedAction(result));

    Console.WriteLine("\nFEEDBACK AND EXPLANATIONS");

    for (int i = 0; i < items.Count; i++)
    {
        var item = items[i];
        bool correct =
            answers[i].SelectedOption == item.CorrectOption;

        Console.WriteLine(
            $"\nQuestion {i + 1}: " +
            (correct ? "Correct" : "Needs revision"));

        Console.WriteLine(
            $"Correct answer: {item.Options[item.CorrectOption]}");

        Console.WriteLine(item.Explanation);
    }

    Console.WriteLine(
        "\nRead the explanations before trying another attempt.");
}

Console.WriteLine("\nSession ended. Thank you for using PathWise AI.");

static int ReadChoice(int maximum)
{
    while (true)
    {
        Console.Write($"Enter a number from 1 to {maximum}: ");
        string? input = Console.ReadLine();

        if (input is null)
            Environment.Exit(0);

        if (int.TryParse(input, out int choice) &&
            choice >= 1 && choice <= maximum)
        {
            return choice;
        }

        Console.WriteLine("Invalid choice. Please try again.");
    }
}

public record QuizItem(
    string Id,
    string Prompt,
    string[] Options,
    int CorrectOption,
    string Explanation
);