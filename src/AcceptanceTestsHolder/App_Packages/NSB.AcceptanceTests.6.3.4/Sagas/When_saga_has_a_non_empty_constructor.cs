﻿namespace NServiceBus.AcceptanceTests.Sagas
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using EndpointTemplates;
    using NUnit.Framework;
    using Persistence.Sql;

    public class When_saga_has_a_non_empty_constructor : NServiceBusAcceptanceTest
    {
        [Test]
        public Task Should_hydrate_and_invoke_the_existing_instance()
        {
            return Scenario.Define<Context>()
                .WithEndpoint<NonEmptySagaCtorEndpt>(b => b.When(session => session.SendLocal(new StartSagaMessage
                {
                    SomeId = IdThatSagaIsCorrelatedOn
                })))
                .Done(c => c.SecondMessageReceived)
                .Run();
        }

        static Guid IdThatSagaIsCorrelatedOn = Guid.NewGuid();

        public class Context : ScenarioContext
        {
            public bool SecondMessageReceived { get; set; }
        }

        public class NonEmptySagaCtorEndpt : EndpointConfigurationBuilder
        {
            public NonEmptySagaCtorEndpt()
            {
                EndpointSetup<DefaultServer>();
            }

            public class TestSaga11 : SqlSaga<TestSagaData11>,
                IAmStartedByMessages<StartSagaMessage>,
                IHandleMessages<OtherMessage>
            {
                protected override string CorrelationPropertyName => nameof(TestSagaData11.SomeId);

                public TestSaga11(Context testContext)
                {
                    this.testContext = testContext;
                }

                public Task Handle(StartSagaMessage message, IMessageHandlerContext context)
                {
                    Data.SomeId = message.SomeId;
                    return context.SendLocal(new OtherMessage
                    {
                        SomeId = message.SomeId
                    });
                }

                public Task Handle(OtherMessage message, IMessageHandlerContext context)
                {
                    testContext.SecondMessageReceived = true;
                    return Task.FromResult(0);
                }

                protected override void ConfigureMapping(IMessagePropertyMapper mapper)
                {
                    mapper.ConfigureMapping<StartSagaMessage>(m => m.SomeId);
                    mapper.ConfigureMapping<OtherMessage>(m => m.SomeId);
                }

                Context testContext;
            }

            public class TestSagaData11 : IContainSagaData
            {
                public virtual Guid SomeId { get; set; }
                public virtual Guid Id { get; set; }
                public virtual string Originator { get; set; }
                public virtual string OriginalMessageId { get; set; }
            }
        }


        public class StartSagaMessage : ICommand
        {
            public Guid SomeId { get; set; }
        }


        public class OtherMessage : ICommand
        {
            public Guid SomeId { get; set; }
        }
    }
}