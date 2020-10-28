# generative-components
__Custom Generative Components Nodes__


Refer to the [wiki](https://github.com/edashbolt/generative-components/wiki) for more information regarding setting up your dev environment and other relevant links


## Summary
This project contains some custom "nodes" for Bentley Systems Generative Components software. It has been open-sourced in the hope that others in the GC community will contribute their own developments, either by improving the existing source code or by adding tools that they have created to further extend the capabilities of this software. Let's collaborate and make this software even more powerful and available for all to use!


## Included Nodes

### ParametricCell:
This node contains the following methods:
* PlaceParametricCell
  
  This will place a parametric cell in the design file. The inputs required are:
  * PlacementPoint (cell origin)
  * PlacementPlane (cell orientation)
  * CellLibraryPath (path to the cell library, manually entered as a string. Ideally we would modify this to also include a FileBrowserDialog)
  * CellDefinitionName (name of the cell definition you want to place. Ideally we would modify this to include a drop down of available definitions in the CellLibraryPath)
  * CellVariation (name of the cell Variation. This can be set to null)
  * CellVariableName (must be a string array due to the limitations of replication of the system.object type in GC update 6)
  * CellVariableValue (as above this must also be a string array. We could change this to be a double instead, but in lieu of using a replicatable system.object type I wanted to keep it as versatile as possible, everything can be cast to string type so this is a short-term solution to the aforementioned issue)
  
  Aside from the items listed above, other improvements that would be beneficial:
  * Optimise the performance by only updating a cell with each specific input that may have changed, rather than rebuild with a completely new cell instance each time. For example, if the placement point changes we simply need to transform the cell rather than place a new instance and update all the cell parameters
  * Optimise performance for updating cell parameters by avoiding iteration; ideally we would create a dictionary to quickly find the keys we need
  
  
* DropParametricCellToOrphanCell

  As the name suggests, this will drop a parametric cell to an orphan cell. The idea behind this workflow is that if you want to obtain the raw geometry of your parametric cell so that you can manipulate the elements downstream in your GC graph, you can use this methods output ".ElementPath" as an input for the delivered Cell.ByElement node method. Currently this tool only processes elements as curves since this was all that I needed for a job, however it would be great to update this to also produce surfaces and solids as well.

### ItemTypes:
This node contains the following methods:
* ReadItems (currently under development, not ready for use)

* WriteItems
  * This node will update existing items attached to the input elements. 
  * If the item does not yet exist on the input element, it will add the new item type to the elemnent/s: In order for this to work the Item Type needs to first be created in the active dgn file.
  * This node currently only adds one new Item Type at a time (although each type can contain as many properties and values as required).
  * Unfortunately due to the limitations of GC the property and value need to be entered as a string array. If only one property / value is required, construct the inputs as a single string array e.g. {'PropertyName'} and {'PropertyValue'}
