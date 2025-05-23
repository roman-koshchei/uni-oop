using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Lab6;

public partial class MainWindow : Window
{
    private CalculateState? calculateState = null;
    private readonly DispatcherTimer timer;
    private string lastStatusBar = "";

    public MainWindow()
    {
        InitializeComponent();
        timer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMilliseconds(1),
            IsEnabled = true
        };
        timer.Tick += UpdateStatusBar_Tick!;
    }

    private void UpdateStatusBar_Tick(object sender, EventArgs e)
    {
        var text = Volatile.Read(ref lastStatusBar);
        statusBarText.Text = text;
    }

    private void SafeUpdateStatusBarText(string message)
    {
        Interlocked.Exchange(ref lastStatusBar, message);
    }

    private void BtnSelectFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            ValidateNames = false,
            Title = "Select a folder"
        };

        if (dialog.ShowDialog() == true)
        {
            var folderPath = dialog.FolderName;
            if (string.IsNullOrEmpty(folderPath))
            {
                MessageBox.Show("Folder path isn't valid", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                txtFolderPath.Text = folderPath;
            }
        }
    }

    private async void BtnCalculateMultiThread_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtFolderPath.Text) || !Directory.Exists(txtFolderPath.Text))
        {
            MessageBox.Show("Please select a valid folder path", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        await CalculateFolderSize(true);
    }

    private async void BtnCalculateSingleThread_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtFolderPath.Text) || !Directory.Exists(txtFolderPath.Text))
        {
            MessageBox.Show("Please select a valid folder path", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        await CalculateFolderSize(false);
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        if (calculateState is not null)
        {
            calculateState.CancellationTokenSource.Cancel();
        }
    }

    private async Task CalculateFolderSize(bool useMultiThread)
    {
        // Reset state
        calculateState = new CalculateState();
        txtResult.Text = "";
        SafeUpdateStatusBarText("Starting calculation...");
        SetControlsState(false);

        var _stopwatch = Stopwatch.StartNew();

        try
        {
            var folderPath = $"{txtFolderPath.Text}";

            SafeUpdateStatusBarText($"Processing folder: {txtFolderPath.Text}");
            if (useMultiThread)
            {
                SafeUpdateStatusBarText("Calculating in multi-threaded mode...");
                await Task.Run(() => CalculateFolderSizeMultiThreadAsync(
                    calculateState, folderPath
                ));
            }
            else
            {
                SafeUpdateStatusBarText("Calculating in single-threaded mode...");
                await Task.Run(() => CalculateFolderSizeSingleThread(
                    calculateState, folderPath
                ));
            }

            _stopwatch.Stop();
            DisplayResults(calculateState, useMultiThread, _stopwatch);
        }
        catch (OperationCanceledException)
        {
            _stopwatch.Stop();
            SafeUpdateStatusBarText("Operation cancelled by user");
            txtResult.Text = "Calculation was cancelled.\n\n" +
                $"Files processed: {calculateState.ProcessedFilesCount}\n" +
                $"Folders processed: {calculateState.ProcessedFoldersCount}\n" +
                $"Execution time: {_stopwatch.Elapsed.TotalSeconds:F2} sec";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SetControlsState(true);
        }
    }

    private async Task CalculateFolderSizeMultiThreadAsync(
        CalculateState state, string folderPath
    )
    {
        try
        {
            var directoryInfo = new DirectoryInfo(folderPath);
            DirectoryInfo[] subdirectories = [];

            subdirectories = directoryInfo.GetDirectories();
            // Start subderictories tasks
            var subdirectoriesTasks = subdirectories.Select(dir => CalculateFolderSizeMultiThreadAsync(
                 state, dir.FullName
             ));

            var files = directoryInfo.GetFiles();
            foreach (var file in files)
            {
                state.CancellationTokenSource.Token.ThrowIfCancellationRequested();
                SafeUpdateStatusBarText($"Processing file: {file}");

                state.AddToTotalSize(file.Length);
                state.IncrementProcessedFilesCount();
            }

            // Await all subderictories
            await Task.WhenAll(subdirectoriesTasks);

            state.IncrementProcessedFoldersCount();
        }
        catch (DirectoryNotFoundException)
        {
            SafeUpdateStatusBarText($"Directory not found: {folderPath}");
        }
        catch (UnauthorizedAccessException)
        {
            SafeUpdateStatusBarText($"Access denied to folder: {folderPath}");
        }
    }

    private void CalculateFolderSizeSingleThread(
        CalculateState state, string folderPath
    )
    {
        try
        {
            // Process files
            foreach (var file in Directory.GetFiles(folderPath))
            {
                state.CancellationTokenSource.Token.ThrowIfCancellationRequested();
                state.IncrementProcessedFilesCount();

                var fileInfo = new FileInfo(file);
                state.AddToTotalSize(fileInfo.Length);

                SafeUpdateStatusBarText($"Processing file: {file}");
            }

            // Process subdirectories
            foreach (var directory in Directory.GetDirectories(folderPath))
            {
                state.CancellationTokenSource.Token.ThrowIfCancellationRequested();
                CalculateFolderSizeSingleThread(state, directory);
            }

            state.IncrementProcessedFoldersCount();
        }
        catch (DirectoryNotFoundException)
        {
            SafeUpdateStatusBarText($"Directory not found: {folderPath}");
        }
        catch (UnauthorizedAccessException)
        {
            SafeUpdateStatusBarText($"Access denied to folder: {folderPath}");
        }
    }

    private void DisplayResults(CalculateState state, bool wasMultiThreaded, Stopwatch stopwatch)
    {
        string mode = wasMultiThreaded ? "multi-threaded" : "single-threaded";

        var sizeInMB = state.TotalSize / (1024.0 * 1024.0);
        var sizeInGB = sizeInMB / 1024.0;

        var resultText = $"Calculation result in {mode} mode:\n\n" +
                       $"Total size: {sizeInMB:F2} MB ({sizeInGB:F2} GB)\n" +
                       $"Files processed: {state.ProcessedFilesCount}\n" +
                       $"Folders processed: {state.ProcessedFoldersCount}\n" +
                       $"Execution time: {stopwatch.Elapsed.TotalSeconds:F2} sec";

        txtResult.Text = resultText;
        SafeUpdateStatusBarText($"Calculation completed. Total size: {sizeInMB:F2} MB");
    }

    private void SetControlsState(bool enabled)
    {
        btnSelectFolder.IsEnabled = enabled;
        btnCalculateMultiThread.IsEnabled = enabled;
        btnCalculateSingleThread.IsEnabled = enabled;
        btnCancel.IsEnabled = !enabled;
    }
}