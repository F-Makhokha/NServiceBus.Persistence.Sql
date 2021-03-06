using System.Collections.Generic;
using NServiceBus.Persistence.Sql.ScriptBuilder;

class Settings
{
    public List<BuildSqlVariant> BuildVariants;
    public string ScriptPromotionPath;
    public bool ProduceOutboxScripts;
    public bool ProduceSubscriptionScripts;
    public bool ProduceTimeoutScripts;
    public bool ProduceSagaScripts;
}