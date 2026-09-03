using System.Collections.Generic;
using System.Linq;

namespace micro_c_app_maui.Models.Reference
{
    // Ported verbatim from the classic app's Models/Reference/ReferenceTree.cs (no Xamarin.Forms
    // dependency, plain C#). The warranty Plans subtree (ReferencePlanData) is not ported yet.
    public class ReferenceTree : IReferenceItem
    {
        public string Name { get; set; } = "";
        public List<IReferenceItem> Nodes { get; set; }

        public ReferenceTree()
        {
            Nodes = new List<IReferenceItem>();
        }

        public ReferenceTree(string name) : this()
        {
            Name = name;
        }

        public void SortNodes()
        {
            var trees = Nodes.Where(n => n is ReferenceTree).OrderBy(n => n.Name).ToList();
            var other = Nodes.Where(n => !(n is ReferenceTree)).OrderBy(n => n.Name).ToList();
            Nodes.Clear();
            Nodes.AddRange(trees);
            Nodes.AddRange(other);

            foreach (var t in trees)
            {
                if (t is ReferenceTree tree)
                {
                    tree.SortNodes();
                }
            }
        }

        // Stops one segment short of the leaf by design - the caller (ReferenceIndexPage) passes the
        // full path including the leaf's own name, uses the returned node as the *parent* folder, and
        // adds the leaf itself as a ReferenceEntry. Recursing on a single remaining segment would
        // create a same-named ReferenceTree folder and nest the real entry one level too deep inside
        // it, not "restore" a dropped leaf.
        public ReferenceTree CreateRoute(IEnumerable<string> path)
        {
            // Was calling path.Count(), path.ElementAt(0), and path.Skip(1) as three separate
            // enumerations - fine for the top-level string[] from Split('.'), but each recursive call
            // passes a lazy Skip() result that isn't list-optimized on netstandard2.0, so every level
            // re-walked the remaining segments 2-3 times. Materialize once instead.
            var segments = path as IReadOnlyList<string> ?? path.ToList();
            if (segments.Count <= 1)
            {
                return this;
            }

            var name = segments[0];
            var rest = segments.Skip(1);
            var node = Nodes.FirstOrDefault(n => n.Name == name);
            if (node == null)
            {
                var next = new ReferenceTree() { Name = name };
                Nodes.Add(next);
                return next.CreateRoute(rest);
            }
            if (node is ReferenceTree tree)
            {
                return tree.CreateRoute(rest);
            }

            return this;
        }

        public IReferenceItem GetNode(IEnumerable<string> path)
        {
            var part = path.FirstOrDefault();
            if (part == null)
            {
                return this;
            }

            var node = Nodes.FirstOrDefault(n => n.Name == part);
            if (node == null)
            {
                return this;
            }

            if (node is ReferenceTree tree)
            {
                return tree.GetNode(path.Skip(1));
            }
            else
            {
                return node;
            }
        }

        public IReferenceItem? SearchForNode(string name)
        {
            var hasNode = Nodes.FirstOrDefault(n => n.Name == name);
            if (hasNode != null)
            {
                return hasNode;
            }

            foreach (ReferenceTree tree in Nodes.Where(n => n is ReferenceTree))
            {
                var searchResult = tree.SearchForNode(name);
                if (searchResult != null)
                {
                    return searchResult;
                }
            }

            return null;
        }
    }
}
