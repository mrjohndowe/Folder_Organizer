using System.IO;
using Microsoft.Data.Sqlite;
using FolderOrganizer.Models;

namespace FolderOrganizer.Services;

public class DatabaseService
{
    private readonly string _databasePath;
    private readonly string _connectionString;

    public DatabaseService()
    {
        var applicationDataDirectory =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "FolderOrganizer");

        Directory.CreateDirectory(applicationDataDirectory);

        _databasePath =
            Path.Combine(
                applicationDataDirectory,
                "folder-organizer.db");

        // If this is the first run, use the bundled database
        // as the starting database if one exists.
        var bundledDatabase =
            Path.Combine(
                AppContext.BaseDirectory,
                "Database",
                "folder-organizer.db");

        if (!File.Exists(_databasePath) &&
            File.Exists(bundledDatabase))
        {
            File.Copy(
                bundledDatabase,
                _databasePath,
                overwrite: false);
        }

        _connectionString =
            new SqliteConnectionStringBuilder
            {
                DataSource = _databasePath,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();
    }

    public async Task<List<OrganizationRun>> GetRunsAsync()
    {
        var runs = new List<OrganizationRun>();

        await using var connection =
            new SqliteConnection(_connectionString);

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText =
        """
    SELECT
        Id,
        RootFolder,
        StartedAt,
        CompletedAt,
        TotalItems,
        PlannedMoves,
        SuccessfulMoves,
        FailedMoves

    FROM OrganizationRuns

    ORDER BY Id DESC;
    """;

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            runs.Add(new OrganizationRun
            {
                Id = reader.GetInt64(0),

                RootFolder =
                    reader.GetString(1),

                StartedAt =
                    DateTime.Parse(reader.GetString(2)),

                CompletedAt =
                    reader.IsDBNull(3)
                        ? null
                        : DateTime.Parse(reader.GetString(3)),

                TotalItems =
                    reader.GetInt32(4),

                PlannedMoves =
                    reader.GetInt32(5),

                SuccessfulMoves =
                    reader.GetInt32(6),

                FailedMoves =
                    reader.GetInt32(7)
            });
        }

        return runs;
    }

    public async Task<List<MoveHistoryEntry>>
        GetMoveHistoryAsync(long runId)
    {
        var entries =
            new List<MoveHistoryEntry>();

        await using var connection =
            new SqliteConnection(_connectionString);

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText =
        """
    SELECT
        Id,
        RunId,
        SourcePath,
        DestinationPath,
        FinalDestinationPath,
        FileType,
        Status,
        ErrorMessage,
        CreatedAt

    FROM MoveHistory

    WHERE RunId = $runId

    ORDER BY Id;
    """;

        command.Parameters.AddWithValue(
            "$runId",
            runId);

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            entries.Add(new MoveHistoryEntry
            {
                Id = reader.GetInt64(0),

                RunId = reader.GetInt64(1),

                SourcePath =
                    reader.GetString(2),

                DestinationPath =
                    reader.IsDBNull(3)
                        ? null
                        : reader.GetString(3),

                FinalDestinationPath =
                    reader.IsDBNull(4)
                        ? null
                        : reader.GetString(4),

                FileType =
                    reader.IsDBNull(5)
                        ? string.Empty
                        : reader.GetString(5),

                Status =
                    reader.GetString(6),

                ErrorMessage =
                    reader.IsDBNull(7)
                        ? null
                        : reader.GetString(7),

                CreatedAt =
                    DateTime.Parse(reader.GetString(8))
            });
        }

        return entries;
    }

    public async Task<HashSet<string>> GetIgnoredPathsAsync()
    {
        var paths =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        await using var connection =
            new SqliteConnection(_connectionString);

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText =
        """
    SELECT FullPath
    FROM IgnoredItems;
    """;

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            paths.Add(reader.GetString(0));
        }

        return paths;
    }

    public async Task AddIgnoredPathAsync(
        string fullPath)
    {
        await using var connection =
            new SqliteConnection(_connectionString);

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText =
        """
    INSERT OR IGNORE INTO IgnoredItems
    (
        FullPath,
        CreatedAt
    )
    VALUES
    (
        $fullPath,
        $createdAt
    );
    """;

        command.Parameters.AddWithValue(
            "$fullPath",
            fullPath);

        command.Parameters.AddWithValue(
            "$createdAt",
            DateTime.UtcNow.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task RemoveIgnoredPathAsync(
        string fullPath)
    {
        await using var connection =
            new SqliteConnection(_connectionString);

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText =
        """
    DELETE FROM IgnoredItems
    WHERE FullPath = $fullPath;
    """;

        command.Parameters.AddWithValue(
            "$fullPath",
            fullPath);

        await command.ExecuteNonQueryAsync();
    }

    public async Task InitializeAsync()
    {
        await using var connection =
            new SqliteConnection(_connectionString);

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText =
        """
        CREATE TABLE IF NOT EXISTS OrganizationRuns
        (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            RootFolder TEXT NOT NULL,
            StartedAt TEXT NOT NULL,
            CompletedAt TEXT,
            TotalItems INTEGER NOT NULL DEFAULT 0,
            PlannedMoves INTEGER NOT NULL DEFAULT 0,
            SuccessfulMoves INTEGER NOT NULL DEFAULT 0,
            FailedMoves INTEGER NOT NULL DEFAULT 0
        );

        CREATE TABLE IF NOT EXISTS MoveHistory
        (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            RunId INTEGER NOT NULL,
            SourcePath TEXT NOT NULL,
            DestinationPath TEXT,
            FinalDestinationPath TEXT,
            FileType TEXT,
            Status TEXT NOT NULL,
            ErrorMessage TEXT,
            CreatedAt TEXT NOT NULL,

            FOREIGN KEY (RunId)
                REFERENCES OrganizationRuns(Id)
        );

        CREATE INDEX IF NOT EXISTS
            IX_MoveHistory_RunId
            ON MoveHistory(RunId);

        CREATE INDEX IF NOT EXISTS
            IX_MoveHistory_SourcePath
            ON MoveHistory(SourcePath);
        """;

        await command.ExecuteNonQueryAsync();
    }

    public async Task<long> StartRunAsync(
        string rootFolder,
        int totalItems,
        int plannedMoves)
    {
        await using var connection =
            new SqliteConnection(_connectionString);

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText =
        """
        INSERT INTO OrganizationRuns
        (
            RootFolder,
            StartedAt,
            TotalItems,
            PlannedMoves
        )
        VALUES
        (
            $rootFolder,
            $startedAt,
            $totalItems,
            $plannedMoves
        );

        SELECT last_insert_rowid();
        """;

        command.Parameters.AddWithValue(
            "$rootFolder",
            rootFolder);

        command.Parameters.AddWithValue(
            "$startedAt",
            DateTime.UtcNow.ToString("O"));

        command.Parameters.AddWithValue(
            "$totalItems",
            totalItems);

        command.Parameters.AddWithValue(
            "$plannedMoves",
            plannedMoves);

        var result =
            await command.ExecuteScalarAsync();

        return Convert.ToInt64(result);
    }

    public async Task RecordMoveAsync(
        long runId,
        string sourcePath,
        string? destinationPath,
        string? finalDestinationPath,
        string fileType,
        string status,
        string? errorMessage = null)
    {
        await using var connection =
            new SqliteConnection(_connectionString);

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText =
        """
        INSERT INTO MoveHistory
        (
            RunId,
            SourcePath,
            DestinationPath,
            FinalDestinationPath,
            FileType,
            Status,
            ErrorMessage,
            CreatedAt
        )
        VALUES
        (
            $runId,
            $sourcePath,
            $destinationPath,
            $finalDestinationPath,
            $fileType,
            $status,
            $errorMessage,
            $createdAt
        );
        """;

        command.Parameters.AddWithValue(
            "$runId",
            runId);

        command.Parameters.AddWithValue(
            "$sourcePath",
            sourcePath);

        command.Parameters.AddWithValue(
            "$destinationPath",
            (object?)destinationPath ?? DBNull.Value);

        command.Parameters.AddWithValue(
            "$finalDestinationPath",
            (object?)finalDestinationPath ?? DBNull.Value);

        command.Parameters.AddWithValue(
            "$fileType",
            fileType);

        command.Parameters.AddWithValue(
            "$status",
            status);

        command.Parameters.AddWithValue(
            "$errorMessage",
            (object?)errorMessage ?? DBNull.Value);

        command.Parameters.AddWithValue(
            "$createdAt",
            DateTime.UtcNow.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task CompleteRunAsync(
        long runId,
        int successfulMoves,
        int failedMoves)
    {
        await using var connection =
            new SqliteConnection(_connectionString);

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText =
        """
        UPDATE OrganizationRuns

        SET
            CompletedAt = $completedAt,
            SuccessfulMoves = $successfulMoves,
            FailedMoves = $failedMoves

        WHERE Id = $runId;
        """;

        command.Parameters.AddWithValue(
            "$completedAt",
            DateTime.UtcNow.ToString("O"));

        command.Parameters.AddWithValue(
            "$successfulMoves",
            successfulMoves);

        command.Parameters.AddWithValue(
            "$failedMoves",
            failedMoves);

        command.Parameters.AddWithValue(
            "$runId",
            runId);

        await command.ExecuteNonQueryAsync();
    }
}