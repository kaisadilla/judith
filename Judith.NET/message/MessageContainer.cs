﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.message;

public class MessageContainer {
    public List<CompilerMessage> Infos { get; init; } = new();
    public List<CompilerMessage> Warnings { get; init; } = new();
    public List<CompilerMessage> Errors { get; init; } = new();

    public bool HasErrors => Errors.Count > 0;

    public void Add (CompilerMessage message) {
        switch (message.Kind) {
            case MessageKind.Information:
                Infos.Add(message);
                break;
            case MessageKind.Warning:
                Warnings.Add(message);
                break;
            case MessageKind.Error:
                Errors.Add(message);
                break;
        }
    }

    public void Add (MessageContainer other) {
        Infos.AddRange(other.Infos);
        Warnings.AddRange(other.Warnings);
        Errors.AddRange(other.Errors);
    }
}
