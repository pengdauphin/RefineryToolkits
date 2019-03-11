﻿using System;
using System.Collections.Generic;
using Dynamo.Graph.Nodes;
using ProtoCore.AST.AssociativeAST;
using System.Linq;
using Newtonsoft.Json;
using System.Windows;
using CoreNodeModels;
using Dynamo.Utilities;

namespace GenerativeToolkit.Dropdown
{
    [NodeName("PlacementMethod")]
    [NodeDescription("An example drop down node.")]
    [NodeCategory("GenerativeToolkit.GenerativeToolkit.Layouts.MaxRectanglesBinPacking")]
    [IsDesignScriptCompatible]
    public class PlacementMethodDropDown : DSDropDownBase
    {
        public PlacementMethodDropDown() : base("item") { }
        // Test Comment!
        // Starting with Dynamo v2.0 you must add Json constructors for all nodeModel
        // dervived nodes to support the move from an Xml to Json file format.  Failing to
        // do so will result in incorrect ports being generated upon serialization/deserialization.
        // This constructor is called when opening a Json graph. We must also pass the deserialized 
        // ports with the json constructor and then call the base class passing the ports as parameters.
        [JsonConstructor]
        public PlacementMethodDropDown(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base("Method", inPorts, outPorts) { }

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            // The Items collection contains the elements
            // that appear in the list. For this example, we
            // clear the list before adding new items, but you
            // can also use the PopulateItems method to add items
            // to the list.

            Items.Clear();

            // Create a number of DynamoDropDownItem objects 
            // to store the items that we want to appear in our list.

            var newItems = new List<DynamoDropDownItem>()
            {
                new DynamoDropDownItem("BSSF", "BSSF"),
                new DynamoDropDownItem("BLSF", "BLSF"),
                new DynamoDropDownItem("BAF","BAF")
            };

            Items.AddRange(newItems);

            // Set the selected index to something other
            // than -1, the default, so that your list
            // has a pre-selection.

            SelectedIndex = 0;
            return SelectionState.Done;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            // Build an AST node for the type of object contained in your Items collection.

            var strNode = AstFactory.BuildStringNode((string)Items[SelectedIndex].Item);
            var assign = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), strNode);

            return new List<AssociativeNode> { assign };
        }
    }
}
