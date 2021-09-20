using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ValidIp {

    [Test]
    public void CorrectIps () {
        Assert.True(LobbyControl.validIp("127.0.0.1"));
        Assert.True(LobbyControl.validIp("192.168.43.77"));
        Assert.True(LobbyControl.validIp("200.45.7.54"));
        Assert.True(LobbyControl.validIp("0.0.0.0"));
    }

    [Test]
    public void IncorrectIps() {
        Assert.False(LobbyControl.validIp(""));
        Assert.False(LobbyControl.validIp("a"));
        Assert.False(LobbyControl.validIp("2"));
        Assert.False(LobbyControl.validIp("2.5"));
        Assert.False(LobbyControl.validIp("2.78.8"));
        Assert.False(LobbyControl.validIp("2.7.7.256"));
        Assert.False(LobbyControl.validIp("700.0.0.0"));
        Assert.False(LobbyControl.validIp("_"));
        Assert.False(LobbyControl.validIp("127.000000000.0.1"));
        Assert.False(LobbyControl.validIp("127.-0.0.1"));
        Assert.False(LobbyControl.validIp("123.64.  2.164"));
        Assert.False(LobbyControl.validIp("☺"));
        Assert.False(LobbyControl.validIp("Φ◘╣■♪Üτ☼ƒ₧"));
    }
}
