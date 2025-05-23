using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lab5;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void SolveButton_Click(object sender, RoutedEventArgs e)
    {
        SolveButton.IsEnabled = false;
        ResultsTextBlock.Text = "Calculating, please wait...";

        try
        {
            PuzzleSolver solver = new();
            PuzzleSolution? solution = await Task.Run(() => solver.Solve());

            DisplaySolution(solution);
        }
        catch (Exception ex)
        {
            ResultsTextBlock.Text = $"An error occurred: {ex.Message}\n\n{ex.StackTrace}";
        }
        finally
        {
            SolveButton.IsEnabled = true;
        }
    }

    private void DisplaySolution(PuzzleSolution? solution)
    {
        StringBuilder sb = new();

        if (solution != null)
        {
            sb.AppendLine("SOLUTION FOUND:");
            sb.AppendLine("-------------------------------------");

            sb.AppendLine("\nDETAILS:");
            foreach (var assignment in solution.Connections)
            {
                sb.AppendLine($"Student from {assignment.From} sends {assignment.Poetry} poetry to student from {assignment.To}");
            }

            sb.AppendLine("\n-------------------------------------");
            sb.AppendLine("FINAL ANSWER:");
            sb.AppendLine($"The volume of Ukrainian poetry was sent by a student from {solution.SenderOfUkrainianPoetry}.");
            sb.AppendLine("-------------------------------------");
        }
        else
        {
            sb.AppendLine("NO SOLUTION FOUND.");
            sb.AppendLine("-------------------------------------");
            sb.AppendLine("Ensure all constraints are correctly interpreted and implemented.");
        }

        ResultsTextBlock.Text = sb.ToString();
    }
}