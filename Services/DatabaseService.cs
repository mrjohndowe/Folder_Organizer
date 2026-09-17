using System.IO;
using Microsoft.Data.Sqlite;

namespace FolderOrganizer.Services;

public class DatabaseService
{
    private readonly string _databasePath;
    private readonly string _connectionString;

    public DatabaseService()
    {
        _databasePath = Path.Combine(
            AppContext.BaseDirectory,
            "Database",
            "folder-organizer.db");

        var databaseDirectory =
            Path.GetDirectoryName(_databasePath);

        if (!string.IsNullOrWhiteSpace(databaseDirectory))
        {
            Directory.CreateDirectory(databaseDirectory);
        }

        _connectionString =
            new SqliteConnectionStringBuilder
            {
                DataSource = _databasePath,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();
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