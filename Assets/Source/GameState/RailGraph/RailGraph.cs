using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RailGraphUnindexed { 
    public List<RailGraphNode> nodes = new List<RailGraphNode>();
    public List<RailGraphEdge> edges = new List<RailGraphEdge>();
	public List<TrafficLight> lights = new List<TrafficLight>();

    public RailGraphUnindexed (RailGraph rg) {
        nodes.Clear();
        edges.Clear();
        lights.Clear();
        nodes.AddRange(rg.nodes);
        edges.AddRange(rg.edges);
        foreach (var v in rg.lights) {
            lights.Add(v.Value);
            foreach (var rn in v.Value.rnodeState) {
                var ls = new TrafficLight.lsSer();
                ls.id = rn.Key; ls.parity = rn.Value.parity;
                lights[lights.Count - 1].rnodeSerialize.Add(ls);
			}
        }
	}
}


public class RailGraph { 
    public List<RailGraphNode> nodes = new List<RailGraphNode>();
    public List<RailGraphEdge> edges = new List<RailGraphEdge>();
    
    public Dictionary<int, RailGraphNode> nodeMap = new Dictionary<int, RailGraphNode>();
    public Dictionary<int, RailGraphEdge> edgeMap = new Dictionary<int, RailGraphEdge>();
    public Dictionary<RailGraphNode, List<RailGraphNode>> stars = new Dictionary<RailGraphNode, List<RailGraphNode>>();
    bool seeded = false;

	public Dictionary<int, TrafficLight> lights = new Dictionary<int, TrafficLight>();

    public RailGraph () { }

    public RailGraph (RailGraphUnindexed rgu) {
        nodes.Clear();
        edges.Clear();
        lights.Clear();
        nodes.AddRange(rgu.nodes);
        edges.AddRange(rgu.edges);
        foreach (var light in rgu.lights) {
            lights.Add(light.roadNodeId, light);
            lights[light.roadNodeId].rnodeState = new Dictionary<int, TrafficLight.LightState>();
            foreach (var rn in light.rnodeSerialize) {
                lights[light.roadNodeId].rnodeState.Add(rn.id, 
                    new TrafficLight.LightState(rn.parity));
			}
        }
        precalc();
	}

    public void precalc () {
        precalculateForwardStars();
        precalculateLengths();
        seed();
	}

    public int edgeHash (int i, int j) { return i + j * 10000; }

    public void seed () {
        seeded = true;
        foreach (var node in nodes) {
            if (!nodeMap.ContainsKey(node.id)) {
                nodeMap.Add(node.id, node);
            }
		}
        foreach (var edge in edges) {
            int key = edgeHash(edge.i, edge.j);
            if (!edgeMap.ContainsKey(key)) {
                edgeMap.Add(key, edge);
            }
		}
	}

    public void precalculateForwardStars () {
        foreach (var node in nodes) {
            stars.Add(node, forwardStar(node));
		}
	}

    public void precalculateLengths () {
        foreach (var edge in edges) {
            var start = getNode(edge.i).pos;
            var end = getNode(edge.j).pos;
            edge.length = (start - end).magnitude;
		}
	}

    public RailGraphNode getNode (int id) {
        if (!seeded) {
            if (!nodeMap.ContainsKey(id)) {
                nodeMap.Add(id, nodes.Find(x => x.id == id));
            }
        }
        return nodeMap[id];
	}

    public RailGraphEdge getEdge (int i, int j) {
        int key = edgeHash(i, j);
        if (!seeded) {
            if (!edgeMap.ContainsKey(key)) {
                edgeMap.Add(key, edges.Find(x => x.i == i && x.j == j));
            }
        }
        return edgeMap[key];
	}
    
    public List<RailGraphNode> backwardStar (RailGraphNode node) {
        var star = new List<RailGraphNode>();
        foreach (var edge in edges) {
            if (edge.j == node.id) star.Add(getNode(edge.i)); 
		}
        return star;
	}

    public List<RailGraphNode> forwardStar (RailGraphNode node) {
        var star = new List<RailGraphNode>();
        foreach (var edge in edges) {
            if (edge.i == node.id) star.Add(getNode(edge.j)); 
		}
        return star;
	}
}
