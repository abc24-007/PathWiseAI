using System;
using System.Collections.Generic;
using System.Linq;

namespace PathWiseAI;

public enum SupportLevel
{
    InsufficientEvidence,
    RequiresSupport,
    Developing,
    Mastered
}

public record Question(
    string Id,
    string Topic,
    int CorrectOption
);

public record Answer(
    string QuestionId,
    int SelectedOption
);

public record TopicResult(
    string Topic,
    int Answered,
    int Correct,
    decimal? Score,
    SupportLevel Level
);

public class LearnerModel
{
    // Provisional thresholds: subject to lecturer review.
    public TopicResult Assess(
        List<Question> questions,
        List<Answer> answers)
    {
        if (questions.Count == 0)
            throw new ArgumentException("Questions are required.");

        if (questions.Select(q => q.Topic).Distinct().Count() != 1)
            throw new ArgumentException(
                "Assess one topic at a time.");

        if (questions.Select(q => q.Id).Distinct().Count()
            != questions.Count)
            throw new ArgumentException("Duplicate question IDs.");

        if (answers.Select(a => a.QuestionId).Distinct().Count()
            != answers.Count)
            throw new ArgumentException("Duplicate answers.");

        if (questions.Any(q =>
            string.IsNullOrWhiteSpace(q.Id) ||
            string.IsNullOrWhiteSpace(q.Topic) ||
            q.CorrectOption < 0 ||
            q.CorrectOption > 3))
            throw new ArgumentException("Invalid question.");

        foreach (var answer in answers)
        {
            if (!questions.Any(q => q.Id == answer.QuestionId))
                throw new ArgumentException("Unknown question.");

            if (answer.SelectedOption < 0 ||
                answer.SelectedOption > 3)
                throw new ArgumentException("Invalid answer option.");
        }

        int correct = answers.Count(answer =>
            questions.Single(q => q.Id == answer.QuestionId)
                .CorrectOption == answer.SelectedOption);

        decimal? score = answers.Count == 0
            ? null
            : 100m * correct / answers.Count;

        SupportLevel level;

        if (answers.Count < 5)
            level = SupportLevel.InsufficientEvidence;
        else if (score >= 80m)
            level = SupportLevel.Mastered;
        else if (score >= 50m)
            level = SupportLevel.Developing;
        else
            level = SupportLevel.RequiresSupport;

        return new TopicResult(
            questions[0].Topic,
            answers.Count,
            correct,
            score,
            level);
    }

    public string GetSuggestedAction(TopicResult result)
    {
        return result.Level switch
        {
            SupportLevel.InsufficientEvidence =>
                "Complete at least five questions for this topic.",

            SupportLevel.RequiresSupport =>
                $"Study foundational explanations for {result.Topic}, " +
                "then complete basic practice.",

            SupportLevel.Developing =>
                $"Revise difficult areas in {result.Topic}, " +
                "then complete targeted practice.",

            SupportLevel.Mastered =>
                $"Try extension practice for {result.Topic}. " +
                "This classification applies to this assessment only.",

            _ => throw new ArgumentException("Invalid support level.")
        };
    }
}