using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab5;

public enum Country
{
    England,
    France,
    Ukraine,
    Sweden,
    Germany
}

public class PuzzleConnection
{
    required public Country From { get; set; }
    required public Country Poetry { get; set; }
    required public Country To { get; set; }
}

public class PuzzleSolution
{
    required public List<PuzzleConnection> Connections { get; init; }
    required public Country SenderOfUkrainianPoetry { get; init; }
}

public class PuzzleSolver
{
    private const int NumCountries = 5;
    private readonly List<Country> allCountries = [.. Enum.GetValues(typeof(Country)).Cast<Country>()];

    public PuzzleSolution? Solve()
    {
        var poetrySentPermutations = GetPermutations(allCountries, NumCountries);
        var recipientPermutations = GetDerangements([.. Enumerable.Range(0, NumCountries)]);

        foreach (var poetrySentAssignment in poetrySentPermutations)
        {
            var poetrySentByStudent = new Country[NumCountries];
            for (int i = 0; i < NumCountries; i++)
            {
                poetrySentByStudent[i] = poetrySentAssignment[i];
            }

            foreach (var recipientAssignmentRaw in recipientPermutations)
            {
                var sentToStudent = new Country[NumCountries];
                bool possibleRecipientAssignment = true;
                for (int i = 0; i < NumCountries; i++)
                {
                    if (i == recipientAssignmentRaw[i])
                    {
                        possibleRecipientAssignment = false;
                        break;
                    }
                    sentToStudent[i] = allCountries[recipientAssignmentRaw[i]];
                }
                if (!possibleRecipientAssignment) continue;

                var poetryReceivedByStudent = new Country[NumCountries];

                for (int senderIdx = 0; senderIdx < NumCountries; senderIdx++)
                {
                    Country poetrySent = poetrySentByStudent[senderIdx];
                    Country receiver = sentToStudent[senderIdx];

                    int receiverIdx = allCountries.IndexOf(receiver);
                    poetryReceivedByStudent[receiverIdx] = poetrySent;
                }

                if (CheckConstraints(poetrySentByStudent, sentToStudent, poetryReceivedByStudent))
                {
                    List<PuzzleConnection> connections = [];
                    for (int i = 0; i < NumCountries; i++)
                    {
                        Country sender = allCountries[i];
                        connections.Add(new PuzzleConnection
                        {
                            From = sender,
                            Poetry = poetrySentByStudent[i],
                            To = sentToStudent[i]
                        });
                        //Console.WriteLine($"{sender} sends -- {poetrySentByStudent[i]} poetry --> to {sentToStudent[i]}");
                    }

                    Country senderOfUkrainianPoetry = Country.England;
                    for (int i = 0; i < NumCountries; i++)
                    {
                        if (poetrySentByStudent[i] == Country.Ukraine)
                        {
                            senderOfUkrainianPoetry = allCountries[i];
                            break;
                        }
                    }
                    //Console.WriteLine($"\nAnswer: The volume of Ukrainian poetry was sent by a student from the country {senderOfUkrainianPoetry}.");
                    return new PuzzleSolution
                    {
                        SenderOfUkrainianPoetry = senderOfUkrainianPoetry,
                        Connections = connections
                    };
                }
            }
        }

        return null;
    }

    private bool CheckConstraints(
        Country[] poetrySentByStudent,
        Country[] sentToStudent,
        Country[] poetryReceivedByStudent
    )
    {
        // Check that student from country A didn't got poetry from country A
        for (int i = 0; i < NumCountries; i++)
        {
            Country student = allCountries[i];
            Country poetryReceived = poetryReceivedByStudent[i];
            if (poetryReceived == student)
            {
                return false;
            }
        }

        // Check that France student sent Sweden poetry to German
        int franceIdx = allCountries.IndexOf(Country.France);
        if (!(poetrySentByStudent[franceIdx] == Country.Sweden && sentToStudent[franceIdx] == Country.Germany))
        {
            return false;
        }

        // Check Ukraine - German logic
        int ukraineIdx = allCountries.IndexOf(Country.Ukraine);
        Country poetryUkrainianReceived = poetryReceivedByStudent[ukraineIdx];
        int senderOfGermanPoetryIdx = allCountries.IndexOf(poetryUkrainianReceived);
        Country poetryUkrainianSent = poetrySentByStudent[ukraineIdx];
        int receiverOfGermanPoetryIdx = allCountries.IndexOf(poetryUkrainianSent);
        if (!(
            poetrySentByStudent[senderOfGermanPoetryIdx] == Country.Germany &&
            sentToStudent[senderOfGermanPoetryIdx] == allCountries[receiverOfGermanPoetryIdx]
        ))
        {
            return false;
        }

        // Check that German poetry wasn't sent by receiver of Ukrainian poetry
        Country studentWhoReceivedUkrainianPoetry = Country.England;
        for (int i = 0; i < NumCountries; i++)
        {
            if (poetryReceivedByStudent[i] == Country.Ukraine)
            {
                studentWhoReceivedUkrainianPoetry = allCountries[i];
                break;
            }
        }
        int studentWhoReceivedUkrainianPoetryIdx = allCountries.IndexOf(studentWhoReceivedUkrainianPoetry);
        if (poetrySentByStudent[studentWhoReceivedUkrainianPoetryIdx] == Country.Germany)
        {
            return false;
        }

        return true;
    }

    private List<List<T>> GetPermutations<T>(List<T> list, int length)
    {
        if (length == 1)
        {
            return [.. list.Select(t => new List<T> { t })];
        }

        var permutations = GetPermutations(list, length - 1);

        return [.. permutations.SelectMany(
            t => list.Where(e => !t.Contains(e)),
            (t1, t2) => t1.Concat([t2]).ToList()
        )];
    }

    private List<List<int>> GetDerangements(List<int> items)
    {
        var allPermutations = GetPermutations(items, items.Count);
        var derangements = new List<List<int>>();
        foreach (var p in allPermutations)
        {
            bool isDerangement = true;
            for (int i = 0; i < p.Count; i++)
            {
                if (p[i] == items[i])
                {
                    isDerangement = false;
                    break;
                }
            }
            if (isDerangement)
            {
                derangements.Add(p);
            }
        }
        return derangements;
    }
}