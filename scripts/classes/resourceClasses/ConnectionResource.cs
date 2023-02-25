using Godot;
using Godot.Collections;
// using System;

public partial class ConnectionResource : Resource
{
    [Export]
    public Dictionary<string, Dictionary<NodePath, string> > Connections = new();

    //so <Name,<NodePath, MethodName> > 

    //structure of the dictionary
    //Joe
    // 'Root/Robot', 'Jump()'
    // 'Root/RigidBody', 'Queue_free()'

    //joe2
    // ect

    // the reason of an extra dict at the start is that you can like name and call by their name
    // i think its neat

    
    
}
