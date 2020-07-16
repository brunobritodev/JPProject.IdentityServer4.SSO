var appInsights = window.appInsights || function (e) {
    var c = {
            config: e
        },
        t = document,
        n = window,
        a = "script",
        r = "AuthenticatedUserContext",
        i = "start",
        o = "stop",
        s = "Track",
        p = s + "Event",
        u = s + "Page",
        d = t.createElement(a);
    d.src = e.url || "https://az416426.vo.msecnd.net/scripts/a/ai.0.js", t.getElementsByTagName(a)[0].parentNode.appendChild(d);
    try {
        c.cookie = t.cookie
    } catch (e) { }

    function f(t) {
        c[t] = function () {
            var e = arguments;
            c.queue.push(function () {
                c[t].apply(c, e)
            })
        }
    }
    c.queue = [], c.version = "1.0";
    for (var g = ["Event", "Exception", "Metric", "PageView", "Trace", "Dependency"]; g.length;) f("track" + g.pop());
    if (f("set" + r), f("clear" + r), f(i + p), f(o + p), f(i + u), f(o + u), f("flush"), !e.disableExceptionTracking) {
        f("_" + (g = "onerror"));
        var h = n[g];
        n[g] = function (e, t, n, a, r) {
            var i = h && h(e, t, n, a, r);
            return !0 !== i && c["_" + g](e, t, n, a, r), i
        }
    }
    return c
}({
    instrumentationKey: "98199f6f-d3c9-412f-acab-aefa8a789ed9"
});
(window.appInsights = appInsights).trackPageView();