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

    public async Task<long> AddCustomRuleAsync(
    string folderName,
    string extensions)
    {
        extensions =
             NormalizeExtensions(extensions);

            ValidateCustomRule(
                folderName,
                extensions);
        // existing code continues...
        {
            await using var connection =
            new SqliteConnection(_connectionString);

            await connection.OpenAsync();

            var priorityCommand =
                connection.CreateCommand();

            priorityCommand.CommandText =
            """
                SELECT COALESCE(MAX(Priority), 0) + 1
                FROM CustomRules;
            """;

            var nextPriority =
                Convert.ToInt32(
                    await priorityCommand.ExecuteScalarAsync());

            var now =
                DateTime.UtcNow.ToString("O");

            var command =
                connection.CreateCommand();

            command.CommandText =
            """
                INSERT INTO CustomRules
                (
                    FolderName,
                    Extensions,
                    Priority,
                    IsEnabled,
                    CreatedAt,
                    UpdatedAt
                )
                VALUES
                (
                    $folderName,
                    $extensions,
                    $priority,
                    1,
                    $createdAt,
                    $updatedAt
                );

                SELECT last_insert_rowid();
            """;

            command.Parameters.AddWithValue(
                "$folderName",
                folderName);

            command.Parameters.AddWithValue(
                "$extensions",
                extensions);

            command.Parameters.AddWithValue(
                "$priority",
                nextPriority);

            command.Parameters.AddWithValue(
                "$createdAt",
                now);

            command.Parameters.AddWithValue(
                "$updatedAt",
                now);

            var result =
                await command.ExecuteScalarAsync();

            return Convert.ToInt64(result);
        }
    }

    public async Task SetCustomRulePriorityAsync(
    long ruleId,
    int requestedPriority)
    {
        await using var connection =
            new SqliteConnection(_connectionString);

        await connection.OpenAsync();

        using var transaction =
            connection.BeginTransaction();

        try
        {
            var selectCommand =
                connection.CreateCommand();

            selectCommand.Transaction = transaction;

            selectCommand.CommandText =
            """
        SELECT Id
        FROM CustomRules
        ORDER BY Priority ASC, Id ASC;
        """;

            var ids = new List<long>();

            await using (
                var reader =
                    await selectCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    ids.Add(reader.GetInt64(0));
                }
            }

            if (!ids.Remove(ruleId))
            {
                return;
            }

            var newPriority =
                Math.Clamp(
                    requestedPriority,
                    1,
                    ids.Count + 1);

            ids.Insert(
                newPriority - 1,
                ruleId);

            for (var index = 0;
                 index < ids.Count;
                 index++)
            {
                var updateCommand =
                    connection.CreateCommand();

                updateCommand.Transaction = transaction;

                updateCommand.CommandText =
                """
            UPDATE CustomRules

            SET
                Priority = $priority,
                UpdatedAt = $updatedAt

            WHERE Id = $id;
            """;

                updateCommand.Parameters.AddWithValue(
                    "$priority",
                    index + 1);

                updateCommand.Parameters.AddWithValue(
                    "$updatedAt",
                    DateTime.UtcNow.ToString("O"));

                updateCommand.Parameters.AddWithValue(
                    "$id",
                    ids[index]);

                await updateCommand.ExecuteNonQueryAsync();
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

   

    public async Task MoveCustomRuleAsync(
    long ruleId,
    int requestedPriority)
    {
        await using var connection =
            new SqliteConnection(_connectionString);

        await connection.OpenAsync();

        using var transaction =
            connection.BeginTransaction();

        try
        {
            // Load every rule in the current display order.
            var selectCommand =
                connection.CreateCommand();

            selectCommand.Transaction = transaction;

            selectCommand.CommandText =
            """
        SELECT Id
        FROM CustomRules
        ORDER BY Priority ASC, Id ASC;
        """;

            var ruleIds = new List<long>();

            await using (
                var reader =
                    await selectCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    ruleIds.Add(reader.GetInt64(0));
                }
            }

            if (!ruleIds.Contains(ruleId))
            {
                throw new InvalidOperationException(
                    "The custom rule no longer exists.");
            }

            // Remove the selected rule from its old position.
            ruleIds.Remove(ruleId);

            // Clamp the requested priority to a valid position.
            var newPriority =
                Math.Clamp(
                    requestedPriority,
                    1,
                    ruleIds.Count + 1);

            // Priority 1 = list index 0.
            ruleIds.Insert(
                newPriority - 1,
                ruleId);

            var now =
                DateTime.UtcNow.ToString("O");

            // Rewrite ALL priorities sequentially.
            for (var index = 0;
                 index < ruleIds.Count;
                 index++)
            {
                var updateCommand =
                    connection.CreateCommand();

                updateCommand.Transaction = transaction;

                updateCommand.CommandText =
                """
            UPDATE CustomRules

            SET
                Priority = $priority,
                UpdatedAt = $updatedAt

            WHERE Id = $id;
            """;

                updateCommand.Parameters.AddWithValue(
                    "$priority",
                    index + 1);

                updateCommand.Parameters.AddWithValue(
                    "$updatedAt",
                    now);

                updateCommand.Parameters.AddWithValue(
                    "$id",
                    ruleIds[index]);

                await updateCommand.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static string NormalizeExtensions(
    string extensions)
    {
        if (string.IsNullOrWhiteSpace(extensions))
        {
            return string.Empty;
        }

        var values =
            extensions.Split(
                [',', ';', ' '],
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries);

        var normalized =
            values
                .Select(value =>
                {
                    var extension =
                        value.Trim().ToLowerInvariant();

                    if (!extension.StartsWith('.'))
                    {
                        extension = "." + extension;
                    }

                    return extension;
                })
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        return string.Join(
            ", ",
            normalized);
    }

    public async Task DeleteCustomRuleAsync(
    long ruleId)
    {
        await using var connection =
            new SqliteConnection(_connectionString);

        await connection.OpenAsync();

        using var transaction =
            connection.BeginTransaction();

        try
        {
            var deleteCommand =
                connection.CreateCommand();

            deleteCommand.Transaction = transaction;

            deleteCommand.CommandText =
            """
        DELETE FROM CustomRules
        WHERE Id = $id;
        """;

            deleteCommand.Parameters.AddWithValue(
                "$id",
                ruleId);

            await deleteCommand.ExecuteNonQueryAsync();

            var selectCommand =
                connection.CreateCommand();

            selectCommand.Transaction = transaction;

            selectCommand.CommandText =
            """
        SELECT Id
        FROM CustomRules
        ORDER BY Priority ASC, Id ASC;
        """;

            var ids = new List<long>();

            await using (
                var reader =
                    await selectCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    ids.Add(reader.GetInt64(0));
                }
            }

            for (var index = 0;
                 index < ids.Count;
                 index++)
            {
                var updateCommand =
                    connection.CreateCommand();

                updateCommand.Transaction = transaction;

                updateCommand.CommandText =
                """
            UPDATE CustomRules

            SET
                Priority = $priority,
                UpdatedAt = $updatedAt

            WHERE Id = $id;
            """;

                updateCommand.Parameters.AddWithValue(
                    "$priority",
                    index + 1);

                updateCommand.Parameters.AddWithValue(
                    "$updatedAt",
                    DateTime.UtcNow.ToString("O"));

                updateCommand.Parameters.AddWithValue(
                    "$id",
                    ids[index]);

                await updateCommand.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateCustomRuleAsync(
    CustomRule rule)
    {
        rule.FolderName =
        rule.FolderName.Trim();

            rule.Extensions =
                NormalizeExtensions(
                    rule.Extensions);

            ValidateCustomRule(
                rule.FolderName,
                rule.Extensions);

        // existing code continues...
        {
            await using var connection =
            new SqliteConnection(_connectionString);

            await connection.OpenAsync();

            var command =
                connection.CreateCommand();

            command.CommandText =
            """
            UPDATE CustomRules

            SET
                FolderName = $folderName,
                Extensions = $extensions,
                Priority = $priority,
                IsEnabled = $isEnabled,
                UpdatedAt = $updatedAt,
                Action = $action

            WHERE Id = $id;
            """;

            command.Parameters.AddWithValue(
                "$folderName",
                rule.FolderName.Trim());

            command.Parameters.AddWithValue(
                "$extensions",
                rule.Extensions.Trim());

            command.Parameters.AddWithValue(
                "$priority",
                rule.Priority);

            command.Parameters.AddWithValue(
                "$isEnabled",
                rule.IsEnabled ? 1 : 0);

            command.Parameters.AddWithValue(
                "$updatedAt",
                DateTime.UtcNow.ToString("O"));

            command.Parameters.AddWithValue(
                "$id",
                rule.Id);

            command.Parameters.AddWithValue(
                "$action",
                (int)rule.Action);

            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task<List<CustomRule>> GetCustomRulesAsync()
    {
        var rules =
            new List<CustomRule>();

        await using var connection =
            new SqliteConnection(_connectionString);

        await connection.OpenAsync();

        var command =
            connection.CreateCommand();

        command.CommandText =
        """
        SELECT
            Id,
            FolderName,
            Extensions,
            Priority,
            IsEnabled,
            CreatedAt,
            UpdatedAt,
            Action
        FROM CustomRules
        ORDER BY Priority ASC, Id ASC;
    """;

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            rules.Add(
                new CustomRule
                {
                    Id =
                        reader.GetInt64(
                            reader.GetOrdinal("Id")),

                    FolderName =
                        reader.GetString(
                            reader.GetOrdinal("FolderName")),

                    Extensions =
                        reader.GetString(
                            reader.GetOrdinal("Extensions")),

                    Priority =
                        reader.GetInt32(
                            reader.GetOrdinal("Priority")),

                    IsEnabled =
                        reader.GetInt32(
                            reader.GetOrdinal("IsEnabled")) != 0,

                    CreatedAt =
                        DateTime.Parse(
                            reader.GetString(
                                reader.GetOrdinal("CreatedAt"))),

                    UpdatedAt =
                        DateTime.Parse(
                            reader.GetString(
                                reader.GetOrdinal("UpdatedAt"))),

                    Action =
                        (RuleAction)reader.GetInt32(
                            reader.GetOrdinal("Action"))
                });
        }

        return rules;
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

    public async Task MarkMoveUndoneAsync(long moveHistoryId)
    {
        await using var connection =
            new SqliteConnection(_connectionString);

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText =
        """
            UPDATE MoveHistory
            SET Status = 'UNDONE', ErrorMessage = NULL
            WHERE Id = $id AND Status = 'SUCCESS';
        """;

        command.Parameters.AddWithValue("$id", moveHistoryId);

        var updated = await command.ExecuteNonQueryAsync();

        if (updated != 1)
        {
            throw new InvalidOperationException(
                "This history entry is no longer available to undo.");
        }
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

    private static void ValidateCustomRule(
    string folderName,
    string extensions)
    {
        if (string.IsNullOrWhiteSpace(folderName))
        {
            throw new InvalidOperationException(
                "Folder name cannot be blank.");
        }

        var trimmedFolderName =
            folderName.Trim();

        if (trimmedFolderName is "." or "..")
        {
            throw new InvalidOperationException(
                "Folder name cannot be . or ..");
        }

        if (trimmedFolderName.IndexOfAny(
                Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new InvalidOperationException(
                "Folder name contains invalid Windows characters.");
        }

        if (trimmedFolderName.Contains(
                Path.DirectorySeparatorChar) ||
            trimmedFolderName.Contains(
                Path.AltDirectorySeparatorChar))
        {
            throw new InvalidOperationException(
                "Folder name must be a single folder name, not a path.");
        }

        if (string.IsNullOrWhiteSpace(extensions))
        {
            return;
        }

        var values =
            extensions.Split(
                [',', ';', ' '],
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries);

        foreach (var value in values)
        {
            var extension =
                value.StartsWith('.')
                    ? value
                    : "." + value;

            if (extension.Length < 2)
            {
                throw new InvalidOperationException(
                    "Each extension must contain a file type.");
            }

            if (extension.Contains('*') ||
                extension.Contains('?') ||
                extension.Contains('\\') ||
                extension.Contains('/'))
            {
                throw new InvalidOperationException(
                    $"Invalid extension: {value}");
            }
        }
    }

    public async Task InitializeAsync()
    {
        await using var connection =
            new SqliteConnection(_connectionString);

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText =
        """
        CREATE TABLE IF NOT EXISTS AppSettings
        (
            SettingKey TEXT PRIMARY KEY,
            SettingValue TEXT NOT NULL
        );

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

        CREATE TABLE IF NOT EXISTS CustomRules
        (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            FolderName TEXT NOT NULL,
            Extensions TEXT NOT NULL,
            Priority INTEGER NOT NULL,
            IsEnabled INTEGER NOT NULL DEFAULT 1,
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT NOT NULL,
            Action INTEGER NOT NULL DEFAULT 0
        );

        CREATE INDEX IF NOT EXISTS
            IX_CustomRules_Priority
            ON CustomRules(Priority);

        CREATE TABLE IF NOT EXISTS SpecialRules
        (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            OrganizeByYear INTEGER NOT NULL DEFAULT 0,
            OrganizeByMonth INTEGER NOT NULL DEFAULT 0,
            UseCreationDate INTEGER NOT NULL DEFAULT 1,
            Extensions TEXT NOT NULL,
            IsEnabled INTEGER NOT NULL DEFAULT 1,
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT NOT NULL
        );

        CREATE INDEX IF NOT EXISTS
            IX_MoveHistory_RunId
            ON MoveHistory(RunId);

        CREATE INDEX IF NOT EXISTS
            IX_MoveHistory_SourcePath
            ON MoveHistory(SourcePath);
        """;

        await command.ExecuteNonQueryAsync();
        await EnsureCustomRulesActionColumnAsync(
            connection);
    }

    private static async Task EnsureCustomRulesActionColumnAsync(
    Microsoft.Data.Sqlite.SqliteConnection connection)
    {
        bool hasActionColumn = false;

        await using (var checkCommand =
            connection.CreateCommand())
        {
            checkCommand.CommandText =
                "PRAGMA table_info(CustomRules);";

            await using var reader =
                await checkCommand.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                string columnName =
                    reader.GetString(1);

                if (string.Equals(
                        columnName,
                        "Action",
                        StringComparison.OrdinalIgnoreCase))
                {
                    hasActionColumn = true;
                    break;
                }
            }
        }

        if (hasActionColumn)
        {
            return;
        }

        await using var alterCommand =
            connection.CreateCommand();

        alterCommand.CommandText =
            """
        ALTER TABLE CustomRules
        ADD COLUMN Action INTEGER NOT NULL DEFAULT 0;
        """;

        await alterCommand.ExecuteNonQueryAsync();
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

    public async Task<string?> GetSettingAsync(
    string key)
    {
        await using var connection =
            new SqliteConnection(_connectionString);

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText =
        """
    SELECT SettingValue
    FROM AppSettings
    WHERE SettingKey = $key;
    """;

        command.Parameters.AddWithValue(
            "$key",
            key);

        var result =
            await command.ExecuteScalarAsync();

        return result?.ToString();
    }

    public async Task SetSettingAsync(
        string key,
        string value)
    {
        await using var connection =
            new SqliteConnection(_connectionString);

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText =
        """
    INSERT INTO AppSettings
    (
        SettingKey,
        SettingValue
    )
    VALUES
    (
        $key,
        $value
    )

    ON CONFLICT(SettingKey)
    DO UPDATE SET
        SettingValue = excluded.SettingValue;
    """;

        command.Parameters.AddWithValue(
            "$key",
            key);

        command.Parameters.AddWithValue(
            "$value",
            value);

        await command.ExecuteNonQueryAsync();
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

    // Special Rule Methods

    public async Task<SpecialRule> GetSpecialRuleAsync()
    {
        await using var connection =
            new SqliteConnection(_connectionString);

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText =
        """
        SELECT
            Id,
            OrganizeByYear,
            OrganizeByMonth,
            UseCreationDate,
            Extensions,
            IsEnabled,
            CreatedAt,
            UpdatedAt
        FROM SpecialRules
        WHERE Id = 1;
        """;

        await using var reader =
            await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new SpecialRule
            {
                Id = reader.GetInt64(0),
                OrganizeByYear = reader.GetInt32(1) != 0,
                OrganizeByMonth = reader.GetInt32(2) != 0,
                UseCreationDate = reader.GetInt32(3) != 0,
                Extensions = reader.GetString(4),
                IsEnabled = reader.GetInt32(5) != 0,
                CreatedAt = DateTime.Parse(reader.GetString(6)),
                UpdatedAt = DateTime.Parse(reader.GetString(7))
            };
        }

        // Create default special rule if none exists
        return await CreateDefaultSpecialRuleAsync();
    }

    public async Task<SpecialRule> UpdateSpecialRuleAsync(
        SpecialRule rule)
    {
        rule.Extensions =
            NormalizeExtensions(rule.Extensions);

        ValidateSpecialRule(rule);

        await using var connection =
            new SqliteConnection(_connectionString);

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText =
        """
        UPDATE SpecialRules
        SET
            OrganizeByYear = $organizeByYear,
            OrganizeByMonth = $organizeByMonth,
            UseCreationDate = $useCreationDate,
            Extensions = $extensions,
            IsEnabled = $isEnabled,
            UpdatedAt = $updatedAt
        WHERE Id = $id;
        """;

        command.Parameters.AddWithValue(
            "$organizeByYear",
            rule.OrganizeByYear ? 1 : 0);

        command.Parameters.AddWithValue(
            "$organizeByMonth",
            rule.OrganizeByMonth ? 1 : 0);

        command.Parameters.AddWithValue(
            "$useCreationDate",
            rule.UseCreationDate ? 1 : 0);

        command.Parameters.AddWithValue(
            "$extensions",
            rule.Extensions);

        command.Parameters.AddWithValue(
            "$isEnabled",
            rule.IsEnabled ? 1 : 0);

        command.Parameters.AddWithValue(
            "$updatedAt",
            DateTime.UtcNow.ToString("O"));

        command.Parameters.AddWithValue(
            "$id",
            rule.Id);

        await command.ExecuteNonQueryAsync();

        return await GetSpecialRuleAsync();
    }

    private async Task<SpecialRule> CreateDefaultSpecialRuleAsync()
    {
        await using var connection =
            new SqliteConnection(_connectionString);

        await connection.OpenAsync();

        var now = DateTime.UtcNow.ToString("O");

        var command = connection.CreateCommand();

        command.CommandText =
        """
        INSERT INTO SpecialRules
        (
            OrganizeByYear,
            OrganizeByMonth,
            UseCreationDate,
            Extensions,
            IsEnabled,
            CreatedAt,
            UpdatedAt
        )
        VALUES
        (
            0,
            0,
            1,
            '',
            1,
            $createdAt,
            $updatedAt
        );

        SELECT last_insert_rowid();
        """;

        command.Parameters.AddWithValue(
            "$createdAt",
            now);

        command.Parameters.AddWithValue(
            "$updatedAt",
            now);

        var result =
            await command.ExecuteScalarAsync();

        return new SpecialRule
        {
            Id = Convert.ToInt64(result),
            OrganizeByYear = false,
            OrganizeByMonth = false,
            UseCreationDate = true,
            Extensions = string.Empty,
            IsEnabled = true,
            CreatedAt = DateTime.Parse(now),
            UpdatedAt = DateTime.Parse(now)
        };
    }

    private static void ValidateSpecialRule(
        SpecialRule rule)
    {
        if (rule.OrganizeByMonth && !rule.OrganizeByYear)
        {
            throw new InvalidOperationException(
                "Organize by month requires organize by year to be enabled.");
        }

        // Validate extensions if provided
        if (!string.IsNullOrWhiteSpace(rule.Extensions))
        {
            var values =
                rule.Extensions.Split(
                    [',', ';', ' '],
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries);

            foreach (var value in values)
            {
                var extension =
                    value.StartsWith('.')
                        ? value
                        : "." + value;

                if (extension.Length < 2)
                {
                    throw new InvalidOperationException(
                        "Each extension must contain a file type.");
                }

                if (extension.Contains('*') ||
                    extension.Contains('?') ||
                    extension.Contains('\\') ||
                    extension.Contains('/'))
                {
                    throw new InvalidOperationException(
                        $"Invalid extension: {value}");
                }
            }
        }
    }
}
