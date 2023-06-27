using System;
using System.Windows;
using System.Collections.Generic;
using System.ComponentModel;
using Bentley.GenerativeComponents;
using Bentley.GenerativeComponents.AddInSupport;
using Bentley.GenerativeComponents.GCScript;
//using Bentley.GenerativeComponents.GCScript.NameScopes;
//using Bentley.GenerativeComponents.GeneralPurpose;
//using Bentley.GenerativeComponents.MicroStation;
using Bentley.GenerativeComponents.View;
using Bentley.DgnPlatformNET;
using Bentley.MstnPlatformNET;
using Bentley.DgnPlatformNET.DgnEC;
//using Bentley.ECObjects.Schema;
//using Bentley.ECObjects.Instance;
using Bentley.GenerativeComponents.ElementBasedNodes;
using Bentley.DgnPlatformNET.Elements;
using Bentley.GenerativeComponents.GCScript.GCTypes;
using Bentley.GenerativeComponents.GeneralPurpose.Collections;
using Bentley.GenerativeComponents.GeneralPurpose;
using Bentley.GenerativeComponents.ScriptEditor.Controls.ExpressionEditor;

namespace GCCommunity
{
    [GCNamespace("User")]
    [GCNodeTypePaletteCategory("Sample Add-In")]
    [GCSummary("Example demonstrating the use of grouped lists used for selecting an input")]
    public class BioChamberNode : ElementBasedNode
    {
        static ExpressionEditorCustomConfiguration GetEECCForAnimal()
        {
            return new ExpressionEditorCustomConfiguration(getScriptChoices);

            IEnumerable<ScriptChoice> getScriptChoices(ExpressionEditor parentExpressionEditor)
            {
                BioChamberNode node = (BioChamberNode)parentExpressionEditor.MeaningOfThis;
                ScriptChoiceList result = node.GetGroupedAnimals();
                return result;
            }
        }

        ScriptChoiceList GetGroupedAnimals()
        {
            // Simulate determining the animals at runtime, based on the current state of this node.
            // (For this example, we're simply hard-coding the result.)

            string[] mammals = { "Bat", "Elephant", "Squirrel", "Whale" };
            string[] fish = { "Fish", "Eel", "Minnow", "Salmon", "Shark" };

            ScriptChoiceList groupChoices = new ScriptChoiceList();
            addGroupChoice("Mammals", mammals);
            addGroupChoice("Fish", fish);

            return groupChoices;

            void addGroupChoice(string groupName, string[] animals)
            {
                ScriptChoice groupChoice = groupChoices.Add(groupName);
                foreach (string animal in animals)
                    groupChoice.AddSubChoice(animal.ToQuotedScriptText());
            }
        }

        [GCDefaultTechnique]
        public NodeUpdateResult Default
        (
            NodeUpdateContext updateContext,
            [GCDgnModelProvider] IPointNode AnchorPoint,
            [GCExpressionEditorCustomConfiguration(nameof(GetEECCForAnimal))] string Animal
        )
        {
            return NodeUpdateResult.Success;
        }
    }
}
