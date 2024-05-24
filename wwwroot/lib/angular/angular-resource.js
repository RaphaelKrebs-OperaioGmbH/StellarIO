﻿/*
 AngularJS v1.8.2
 (c) 2010-2020 Google LLC. http://angularjs.org
 License: MIT
*/
(function (T, a) {
    'use strict'; function M(m, f) { f = f || {}; a.forEach(f, function (a, d) { delete f[d] }); for (var d in m) !m.hasOwnProperty(d) || "$" === d.charAt(0) && "$" === d.charAt(1) || (f[d] = m[d]); return f } var B = a.$$minErr("$resource"), H = /^(\.[a-zA-Z_$@][0-9a-zA-Z_$@]*)+$/; a.module("ngResource", ["ng"]).info({ angularVersion: "1.8.2" }).provider("$resource", function () {
        var m = /^https?:\/\/\[[^\]]*][^/]*/, f = this; this.defaults = {
            stripTrailingSlashes: !0, cancellable: !1, actions: {
                get: { method: "GET" }, save: { method: "POST" }, query: {
                    method: "GET",
                    isArray: !0
                }, remove: { method: "DELETE" }, "delete": { method: "DELETE" }
            }
        }; this.$get = ["$http", "$log", "$q", "$timeout", function (d, F, G, N) {
            function C(a, d) { this.template = a; this.defaults = n({}, f.defaults, d); this.urlParams = {} } var O = a.noop, r = a.forEach, n = a.extend, R = a.copy, P = a.isArray, D = a.isDefined, x = a.isFunction, I = a.isNumber, y = a.$$encodeUriQuery, S = a.$$encodeUriSegment; C.prototype = {
                setUrlParams: function (a, d, f) {
                    var g = this, c = f || g.template, s, h, n = "", b = g.urlParams = Object.create(null); r(c.split(/\W/), function (a) {
                        if ("hasOwnProperty" ===
                            a) throw B("badname"); !/^\d+$/.test(a) && a && (new RegExp("(^|[^\\\\]):" + a + "(\\W|$)")).test(c) && (b[a] = { isQueryParamValue: (new RegExp("\\?.*=:" + a + "(?:\\W|$)")).test(c) })
                    }); c = c.replace(/\\:/g, ":"); c = c.replace(m, function (b) { n = b; return "" }); d = d || {}; r(g.urlParams, function (b, a) {
                        s = d.hasOwnProperty(a) ? d[a] : g.defaults[a]; D(s) && null !== s ? (h = b.isQueryParamValue ? y(s, !0) : S(s), c = c.replace(new RegExp(":" + a + "(\\W|$)", "g"), function (b, a) { return h + a })) : c = c.replace(new RegExp("(/?):" + a + "(\\W|$)", "g"), function (b, a, e) {
                            return "/" ===
                                e.charAt(0) ? e : a + e
                        })
                    }); g.defaults.stripTrailingSlashes && (c = c.replace(/\/+$/, "") || "/"); c = c.replace(/\/\.(?=\w+($|\?))/, "."); a.url = n + c.replace(/\/(\\|%5C)\./, "/."); r(d, function (b, c) { g.urlParams[c] || (a.params = a.params || {}, a.params[c] = b) })
                }
            }; return function (m, y, z, g) {
                function c(b, c) {
                    var d = {}; c = n({}, y, c); r(c, function (c, f) {
                        x(c) && (c = c(b)); var e; if (c && c.charAt && "@" === c.charAt(0)) {
                            e = b; var k = c.substr(1); if (null == k || "" === k || "hasOwnProperty" === k || !H.test("." + k)) throw B("badmember", k); for (var k = k.split("."), h = 0,
                                n = k.length; h < n && a.isDefined(e); h++) { var g = k[h]; e = null !== e ? e[g] : void 0 }
                        } else e = c; d[f] = e
                    }); return d
                } function s(b) { return b.resource } function h(b) { M(b || {}, this) } var Q = new C(m, g); z = n({}, f.defaults.actions, z); h.prototype.toJSON = function () { var b = n({}, this); delete b.$promise; delete b.$resolved; delete b.$cancelRequest; return b }; r(z, function (b, a) {
                    var f = !0 === b.hasBody || !1 !== b.hasBody && /^(POST|PUT|PATCH)$/i.test(b.method), g = b.timeout, m = D(b.cancellable) ? b.cancellable : Q.defaults.cancellable; g && !I(g) && (F.debug("ngResource:\n  Only numeric values are allowed as `timeout`.\n  Promises are not supported in $resource, because the same value would be used for multiple requests. If you are looking for a way to cancel requests, you should use the `cancellable` option."),
                        delete b.timeout, g = null); h[a] = function (e, k, J, y) {
                            function z(a) { p.catch(O); null !== u && u.resolve(a) } var K = {}, v, t, w; switch (arguments.length) { case 4: w = y, t = J; case 3: case 2: if (x(k)) { if (x(e)) { t = e; w = k; break } t = k; w = J } else { K = e; v = k; t = J; break } case 1: x(e) ? t = e : f ? v = e : K = e; break; case 0: break; default: throw B("badargs", arguments.length); }var E = this instanceof h, l = E ? v : b.isArray ? [] : new h(v), q = {}, C = b.interceptor && b.interceptor.request || void 0, D = b.interceptor && b.interceptor.requestError || void 0, F = b.interceptor && b.interceptor.response ||
                                s, H = b.interceptor && b.interceptor.responseError || G.reject, I = t ? function (a) { t(a, A.headers, A.status, A.statusText) } : void 0; w = w || void 0; var u, L, A; r(b, function (a, b) { switch (b) { default: q[b] = R(a); case "params": case "isArray": case "interceptor": case "cancellable": } }); !E && m && (u = G.defer(), q.timeout = u.promise, g && (L = N(u.resolve, g))); f && (q.data = v); Q.setUrlParams(q, n({}, c(v, b.params || {}), K), b.url); var p = G.resolve(q).then(C).catch(D).then(d), p = p.then(function (c) {
                                    var e = c.data; if (e) {
                                        if (P(e) !== !!b.isArray) throw B("badcfg",
                                            a, b.isArray ? "array" : "object", P(e) ? "array" : "object", q.method, q.url); if (b.isArray) l.length = 0, r(e, function (a) { "object" === typeof a ? l.push(new h(a)) : l.push(a) }); else { var d = l.$promise; M(e, l); l.$promise = d }
                                    } c.resource = l; A = c; return F(c)
                                }, function (a) { a.resource = l; A = a; return H(a) }), p = p["finally"](function () { l.$resolved = !0; !E && m && (l.$cancelRequest = O, N.cancel(L), u = L = q.timeout = null) }); p.then(I, w); return E ? p : (l.$promise = p, l.$resolved = !1, m && (l.$cancelRequest = z), l)
                        }; h.prototype["$" + a] = function (b, c, d) {
                            x(b) && (d = c, c =
                                b, b = {}); b = h[a].call(this, b, this, c, d); return b.$promise || b
                        }
                }); return h
            }
        }]
    })
})(window, window.angular);
//# sourceMappingURL=angular-resource.min.js.map
