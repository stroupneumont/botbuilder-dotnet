﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// An implementation of the IBot interface intended for further subclassing.
    /// Derive from this class to plug in code to handle particular Activity types.
    /// Pre and post processing of Activities can be plugged in by deriving and calling
    /// the base class implementation.
    /// </summary>
    public class ActivityHandler : IBot
    {
        /// <summary>
        /// The OnTurnAsync function is called by the Adapter (for example, the <see cref="BotFrameworkHttpAdapter"/>)
        /// at runtime in order to process an inbound Activity.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public virtual Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (turnContext.Activity == null)
            {
                throw new ArgumentException($"{nameof(turnContext)} must have non-null Activity.");
            }

            if (turnContext.Activity.Type == null)
            {
                throw new ArgumentException($"{nameof(turnContext)}.Activity must have non-null Type.");
            }

            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.Message:
                    return OnMessageActivityAsync(new DelegatingTurnContext<IMessageActivity>(turnContext), cancellationToken);

                case ActivityTypes.ConversationUpdate:
                    return OnConversationUpdateActivityAsync(new DelegatingTurnContext<IConversationUpdateActivity>(turnContext), cancellationToken);

                case ActivityTypes.Event:
                    return OnEventActivityAsync(new DelegatingTurnContext<IEventActivity>(turnContext), cancellationToken);

                case ActivityTypes.ContactRelationUpdate:
                    return OnContactRelationUpdateActivityAsync(new DelegatingTurnContext<IContactRelationUpdateActivity>(turnContext), cancellationToken);

                case ActivityTypes.Invoke:
                    return OnInvokeActivityAsync(new DelegatingTurnContext<IInvokeActivity>(turnContext), cancellationToken);

                case ActivityTypes.EndOfConversation:
                    return OnEndOfConversationActivityAsync(new DelegatingTurnContext<IEndOfConversationActivity>(turnContext), cancellationToken);

                case ActivityTypes.DeleteUserData:
                    return OnDeleteUserDataActivityAsync(turnContext, cancellationToken);

                case ActivityTypes.MessageUpdate:
                    return OnMessageUpdateActivityAsync(new DelegatingTurnContext<IMessageUpdateActivity>(turnContext), cancellationToken);

                case ActivityTypes.MessageDelete:
                    return OnMessageDeleteActivityAsync(new DelegatingTurnContext<IMessageDeleteActivity>(turnContext), cancellationToken);

                case ActivityTypes.MessageReaction:
                    return OnMessageReactionActivityAsync(new DelegatingTurnContext<IMessageReactionActivity>(turnContext), cancellationToken);

                case ActivityTypes.InstallationUpdate:
                    return OnInstallationUpdateActivityAsync(new DelegatingTurnContext<IInstallationUpdateActivity>(turnContext), cancellationToken);

                case ActivityTypes.Typing:
                    return OnTypingActivityAsync(new DelegatingTurnContext<ITypingActivity>(turnContext), cancellationToken);

                case ActivityTypes.Handoff:
                    return OnHandoffActivityAsync(new DelegatingTurnContext<IHandoffActivity>(turnContext), cancellationToken);

                default:
                    return OnUnrecognizedActivityTypeAsync(turnContext, cancellationToken);
            }
        }

        protected virtual Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.MembersAdded != null)
            {
                if (turnContext.Activity.MembersAdded.Count(m => m.Id != turnContext.Activity.Recipient?.Id) > 1)
                {
                    return OnMembersAddedAsync(turnContext.Activity.MembersAdded, turnContext, cancellationToken);
                }
            }
            else if (turnContext.Activity.MembersRemoved != null)
            {
                if (turnContext.Activity.MembersRemoved.Count(m => m.Id != turnContext.Activity.Recipient?.Id) > 1)
                {
                    return OnMembersRemovedAsync(turnContext.Activity.MembersRemoved, turnContext, cancellationToken);
                }
            }

            return Task.CompletedTask;
        }

        protected virtual Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnMembersRemovedAsync(IList<ChannelAccount> membersRemoved, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Name != null)
            {
                switch (turnContext.Activity.Name)
                {
                    case "tokens/response":
                        return OnTokenResponseEventAsync(turnContext, cancellationToken);

                    case "createConversation":
                        return OnCreateConversationAsync(turnContext, cancellationToken);

                    case "continueConversation":
                        return OnContinueConversationAsync(turnContext, cancellationToken);
                }
            }

            return OnEventAsync(turnContext, cancellationToken);
        }

        protected virtual Task OnTokenResponseEventAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            // If using the OAuthPrompt override this method to forward this Activity to the Dialog.
            return Task.CompletedTask;
        }

        protected virtual Task OnCreateConversationAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnContinueConversationAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnEventAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnContactRelationUpdateActivityAsync(ITurnContext<IContactRelationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual async Task OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            InvokeResponse invokeResponse;
            if (turnContext.Activity.Name == "signin/verifyState")
            {
                invokeResponse = await OnTeamsVerificationInvokeAsync(turnContext, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                invokeResponse = await OnInvokeAsync(turnContext, cancellationToken).ConfigureAwait(false);
            }

            if (invokeResponse != null)
            {
                var invokeResponseActivity = new Activity { Type = ActivityTypesEx.InvokeResponse, Value = invokeResponse };
                await turnContext.SendActivityAsync(invokeResponseActivity, cancellationToken).ConfigureAwait(false);
            }
        }

        protected virtual Task<InvokeResponse> OnTeamsVerificationInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            // If using the OAuthPrompt override this method to forward this Activity to the Dialog.
            return Task.FromResult(new InvokeResponse { Status = (int)HttpStatusCode.NotImplemented });
        }

        protected virtual Task<InvokeResponse> OnInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(new InvokeResponse { Status = (int)HttpStatusCode.NotImplemented });
        }

        protected virtual Task OnEndOfConversationActivityAsync(ITurnContext<IEndOfConversationActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnDeleteUserDataActivityAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnMessageUpdateActivityAsync(ITurnContext<IMessageUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnMessageDeleteActivityAsync(ITurnContext<IMessageDeleteActivity> turnContext, CancellationToken cancellationToken) => Task.CompletedTask;

        protected virtual Task OnMessageReactionActivityAsync(ITurnContext<IMessageReactionActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.ReactionsAdded != null)
            {
                return OnMessageReactionsAddedAsync(turnContext, cancellationToken);
            }
            else if (turnContext.Activity.ReactionsRemoved != null)
            {
                return OnMessageReactionsRemovedAsync(turnContext, cancellationToken);
            }

            return Task.CompletedTask;
        }

        protected virtual Task OnMessageReactionsAddedAsync(ITurnContext<IMessageReactionActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnMessageReactionsRemovedAsync(ITurnContext<IMessageReactionActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnInstallationUpdateActivityAsync(ITurnContext<IInstallationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnTypingActivityAsync(ITurnContext<ITypingActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnHandoffActivityAsync(ITurnContext<IHandoffActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnUnrecognizedActivityTypeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// A TurnContext with a strongly typed Activity property that wraps an untyped inner TurnContext.
        /// </summary>
        /// <typeparam name="T">An IActivity derived type, that is one of IMessageActivity, IConversationUpdateActivity etc.</typeparam>
        private class DelegatingTurnContext<T> : ITurnContext<T>
            where T : IActivity
        {
            private ITurnContext _innerTurnContext;

            public DelegatingTurnContext(ITurnContext innerTurnContext)
            {
                _innerTurnContext = innerTurnContext;
            }

            T ITurnContext<T>.Activity => (T)(IActivity)_innerTurnContext.Activity;

            public BotAdapter Adapter => _innerTurnContext.Adapter;

            public TurnContextStateCollection TurnState => _innerTurnContext.TurnState;

            public Activity Activity => _innerTurnContext.Activity;

            public bool Responded => _innerTurnContext.Responded;

            public Task DeleteActivityAsync(string activityId, CancellationToken cancellationToken = default(CancellationToken))
                => _innerTurnContext.DeleteActivityAsync(activityId, cancellationToken);

            public Task DeleteActivityAsync(ConversationReference conversationReference, CancellationToken cancellationToken = default(CancellationToken))
                => _innerTurnContext.DeleteActivityAsync(conversationReference, cancellationToken);

            public ITurnContext OnDeleteActivity(DeleteActivityHandler handler)
                => _innerTurnContext.OnDeleteActivity(handler);

            public ITurnContext OnSendActivities(SendActivitiesHandler handler)
                => _innerTurnContext.OnSendActivities(handler);

            public ITurnContext OnUpdateActivity(UpdateActivityHandler handler)
                => _innerTurnContext.OnUpdateActivity(handler);

            public Task<ResourceResponse[]> SendActivitiesAsync(IActivity[] activities, CancellationToken cancellationToken = default(CancellationToken))
                => _innerTurnContext.SendActivitiesAsync(activities, cancellationToken);

            public Task<ResourceResponse> SendActivityAsync(string textReplyToSend, string speak = null, string inputHint = "acceptingInput", CancellationToken cancellationToken = default(CancellationToken))
                => _innerTurnContext.SendActivityAsync(textReplyToSend, speak, inputHint, cancellationToken);

            public Task<ResourceResponse> SendActivityAsync(IActivity activity, CancellationToken cancellationToken = default(CancellationToken))
                => _innerTurnContext.SendActivityAsync(activity, cancellationToken);

            public Task<ResourceResponse> UpdateActivityAsync(IActivity activity, CancellationToken cancellationToken = default(CancellationToken))
                => _innerTurnContext.UpdateActivityAsync(activity, cancellationToken);
        }
    }
}
