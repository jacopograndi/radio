using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskList : IEquatable<TaskList> {
    public List<Task> tasks = new List<Task>();

    public void addTask(Task task) {
        task.id = tasks.Count;
        tasks.Add(task);
    }

    public Task fromId (int id) {
        return tasks.Find(x => x.id == id);
    }

    public void serialize(StreamSerializer serializer) {
        serializer.append(tasks.Count);
        foreach (Task task in tasks) { task.serialize(serializer); }
    }

    public void deserialize (StreamSerializer deserializer) {
        tasks.Clear();
        int count = deserializer.getNextInt();
        for (int i=0; i<count; i++) {
            Task task = new Task();
            task.deserialize(deserializer);
            tasks.Add(task);
        }
    }

    public override bool Equals(object obj) {
        return Equals(obj as TaskList);
    }

    public bool Equals(TaskList other) {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (tasks.Count != other.tasks.Count) return false;
        foreach (Task task in tasks) {
            if (!other.tasks.Contains(task)) return false;
        }
        return true;
    }
}
