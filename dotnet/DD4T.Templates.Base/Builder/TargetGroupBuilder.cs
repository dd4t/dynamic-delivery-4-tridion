using System.Collections.Generic;
using DD4T.ContentModel;
using DD4T.Templates.Base.Utils;
using Tridion.ContentManager.AudienceManagement;
using Condition = DD4T.ContentModel.Condition;
using ConditionOperator = DD4T.ContentModel.ConditionOperator;
using CustomerCharacteristicCondition = Tridion.ContentManager.AudienceManagement.CustomerCharacteristicCondition;
using Dynamic = DD4T.ContentModel;
using NumericalConditionOperator = DD4T.ContentModel.NumericalConditionOperator;
using Tcm = Tridion.ContentManager.AudienceManagement;

namespace DD4T.Templates.Base.Builder
{
    ///<summary>
    /// TargetGroupBuilder is responsible for mapping a Tridion Target Group and associated conditions to a DD4T Content Model Target Group and conditions
    /// 2012-10-05
    /// Author: Robert Stevenson-Leggett
    ///</summary>
    public class TargetGroupBuilder
    {
        /// <summary>
        /// Build a DD4T Target group from a AM Target Group
        /// </summary>
        public static Dynamic.TargetGroup BuildTargetGroup(Tridion.ContentManager.AudienceManagement.TargetGroup targetGroup)
        {
            var tg = new Dynamic.TargetGroup
            {
                Conditions = MapConditions(targetGroup.Conditions),
                Description = targetGroup.Description,
                Id = targetGroup.Id,
                OwningPublication = PublicationBuilder.BuildPublication(targetGroup.OwningRepository),
                Publication = PublicationBuilder.BuildPublication(targetGroup.ContextRepository),
                PublicationId = targetGroup.ContextRepository.Id,
                Title = targetGroup.Title
            };
            return tg;
        }

        /// <summary>
        /// Map the conditions from a Component Presentaton to DD4T conditions
        /// </summary>
        public static List<Dynamic.Condition> MapTargetGroupConditions(IList<Tcm.TargetGroupCondition> componentPresentationConditions)
        {
            var mappedConditions = new List<Dynamic.Condition>();
            foreach (var componentPresentationCondition in componentPresentationConditions)
            {
                mappedConditions.AddRange(MapConditions(componentPresentationCondition.TargetGroup.Conditions));
            }
            return mappedConditions;
        }

        private static List<Condition> MapConditions(IList<Tridion.ContentManager.AudienceManagement.Condition> conditions)
        {
            var mappedConditions = new List<Dynamic.Condition>();
            foreach (var condition in conditions)
            {
                if (condition is TrackingKeyCondition)
                {
                    mappedConditions.Add(MapTrackingKeyCondition((TrackingKeyCondition)condition));
                }
                else if (condition is Tcm.TargetGroupCondition)
                {
                    GeneralUtils.TimedLog("No support for TargetGroupCondition yet");
                    mappedConditions.Add(MapTargetGroupCondition((Tcm.TargetGroupCondition)condition));
                }
                else if (condition is Tcm.CustomerCharacteristicCondition)
                {
                    mappedConditions.Add(MapCustomerCharacteristicCondition((Tcm.CustomerCharacteristicCondition)condition));
                }
                else
                {
                    GeneralUtils.TimedLog("Condition of type: " + condition.GetType().FullName + " was not supported by the mapping code.");
                }
            }
            return mappedConditions;
        }

        private static Dynamic.CustomerCharacteristicCondition MapCustomerCharacteristicCondition(CustomerCharacteristicCondition condition)
        {
            var newCondition = new Dynamic.CustomerCharacteristicCondition()
                                   {
                                       Value = condition.Value,
                                       Operator = (ConditionOperator)condition.Operator,
                                       Name = condition.Name,
                                       Negate = condition.Negate
                                   };
            return newCondition;
        }

        private static Dynamic.TargetGroupCondition MapTargetGroupCondition(Tcm.TargetGroupCondition targetGroupCondition)
        {
            var newCondition = new Dynamic.TargetGroupCondition()
                                   {
                                       TargetGroup = BuildTargetGroup(targetGroupCondition.TargetGroup),
                                       Negate = targetGroupCondition.Negate
                                   };
            return newCondition;
        }

        private static KeywordCondition MapTrackingKeyCondition(TrackingKeyCondition trackingKeyCondition)
        {
            var newCondition = new KeywordCondition
                                   {
                                       Keyword = KeywordBuilder.BuildKeyword(trackingKeyCondition.Keyword),
                                       Operator = (NumericalConditionOperator)trackingKeyCondition.Operator,
                                       Negate = true,
                                       Value = trackingKeyCondition.Value
                                   };
            return newCondition;
        }
    }
}