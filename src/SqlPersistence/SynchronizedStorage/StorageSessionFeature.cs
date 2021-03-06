﻿using System;
using Newtonsoft.Json;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Persistence.Sql;
using NServiceBus.Sagas;
using NServiceBus.Settings;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

class StorageSessionFeature : Feature
{

    protected override void Setup(FeatureConfigurationContext context)
    {
        var settings = context.Settings;
        ValidateSagaOutboxCombo(settings);

        var sqlVariant = settings.GetSqlVariant();
        var infoCache = BuildSagaInfoCache(sqlVariant, settings);
        var container = context.Container;
        var connectionBuilder = settings.GetConnectionBuilder();

        container.ConfigureComponent(() => new SynchronizedStorage(connectionBuilder, infoCache), DependencyLifecycle.SingleInstance);
        container.ConfigureComponent(() => new StorageAdapter(connectionBuilder, infoCache), DependencyLifecycle.SingleInstance);
        var isSagasEnabledForSqlPersistence = settings.IsFeatureActive(typeof(SqlSagaFeature));
        if (isSagasEnabledForSqlPersistence)
        {
            var sagaPersister = new SagaPersister(infoCache, sqlVariant);
            container.ConfigureComponent<ISagaPersister>(() => sagaPersister, DependencyLifecycle.SingleInstance);
        }
    }

    static void ValidateSagaOutboxCombo(ReadOnlySettings settings)
    {
        var isOutboxEnabled = settings.IsFeatureActive(typeof(Outbox));
        var isSagasEnabled = settings.IsFeatureActive(typeof(Sagas));
        if (!isOutboxEnabled || !isSagasEnabled)
        {
            return;
        }
        var isSagasEnabledForSqlPersistence = settings.IsFeatureActive(typeof(SqlSagaFeature));
        var isOutboxEnabledForSqlPersistence = settings.IsFeatureActive(typeof(SqlOutboxFeature));
        if (isSagasEnabledForSqlPersistence && isOutboxEnabledForSqlPersistence)
        {
            return;
        }
        throw new Exception("Sql Persistence must be enable for either both Sagas and Outbox, or neither.");
    }

    static SagaInfoCache BuildSagaInfoCache(SqlVariant sqlVariant, ReadOnlySettings settings)
    {
#pragma warning disable 618
        var commandBuilder = new SagaCommandBuilder(sqlVariant);
#pragma warning restore 618
        var jsonSerializerSettings = SagaSettings.GetJsonSerializerSettings(settings);
        var jsonSerializer = BuildJsonSerializer(jsonSerializerSettings);
        var readerCreator = SagaSettings.GetReaderCreator(settings);
        if (readerCreator == null)
        {
            readerCreator = reader => new JsonTextReader(reader);
        }
        var writerCreator = SagaSettings.GetWriterCreator(settings);
        if (writerCreator == null)
        {
            writerCreator = writer => new JsonTextWriter(writer);
        }
        var nameFilter = SagaSettings.GetNameFilter(settings);
        if (nameFilter == null)
        {
            nameFilter = sagaName => sagaName;
        }
        var versionDeserializeBuilder = SagaSettings.GetVersionSettings(settings);
        var tablePrefix = settings.GetTablePrefix();
        var schema = settings.GetSchema();
        return new SagaInfoCache(
            versionSpecificSettings: versionDeserializeBuilder,
            jsonSerializer: jsonSerializer,
            readerCreator: readerCreator,
            writerCreator: writerCreator,
            commandBuilder: commandBuilder,
            tablePrefix: tablePrefix,
            schema: schema,
            sqlVariant: sqlVariant,
            metadataCollection: settings.Get<SagaMetadataCollection>(),
            nameFilter: nameFilter);
    }

    static JsonSerializer BuildJsonSerializer(JsonSerializerSettings jsonSerializerSettings)
    {
        if (jsonSerializerSettings == null)
        {
            return Serializer.JsonSerializer;
        }
        return JsonSerializer.Create(jsonSerializerSettings);
    }
}