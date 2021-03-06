using System.IO;
using NServiceBus.Persistence.Sql.ScriptBuilder;

class OutboxWriter
{
    public static void WriteOutboxScript(string scriptPath, BuildSqlVariant sqlVariant)
    {
        var createPath = Path.Combine(scriptPath, "Outbox_Create.sql");
        File.Delete(createPath);
        using (var writer = File.CreateText(createPath))
        {
            OutboxScriptBuilder.BuildCreateScript(writer, sqlVariant);
        }
        var dropPath = Path.Combine(scriptPath, "Outbox_Drop.sql");
        File.Delete(dropPath);
        using (var writer = File.CreateText(dropPath))
        {
            OutboxScriptBuilder.BuildDropScript(writer, sqlVariant);
        }
    }
}