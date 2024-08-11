// namespace NodeEditorFramework
// {
//     public class Connection
//     {
//         public IPortLegacy SourcePortLegacy { get; }
//         public IPortLegacy TargetPortLegacy { get; }
//
//         public Connection(IPortLegacy sourcePortLegacy, IPortLegacy targetPortLegacy)
//         {
//             SourcePortLegacy = sourcePortLegacy;
//             TargetPortLegacy = targetPortLegacy;
//         }
//         
//         
//         public void Delete()
//         {
//             SourcePortLegacy.RemoveConnection(this);
//             TargetPortLegacy.RemoveConnection(this);
//         }
//     }
// }