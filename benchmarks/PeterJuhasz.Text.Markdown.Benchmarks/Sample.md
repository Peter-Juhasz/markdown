# Orbital Mechanics Toolkit

A small, dependency free library for propagating satellite orbits, computing ground
tracks and planning maneuvers. It is designed for *simulation*, **visualization** and
mission planning workloads, and it runs on any platform with a modern runtime.

Questions? Write to support@example.org or open an issue.

## Installation

Install the package from the public feed:

```sh
dotnet add package Orbital.Mechanics
```

Then reference the namespace and you are ready to go:

```csharp
using Orbital.Mechanics;

var elements = new KeplerianElements(
    semiMajorAxis: 6_878_137.0,
    eccentricity: 0.0012,
    inclination: 51.64
);

var state = Propagator.Propagate(elements, TimeSpan.FromMinutes(45));
Console.WriteLine($"{state.Position} {state.Velocity}");
```

## Feature overview

| Feature | Status | Since | Notes |
| --- | :-: | --: | --- |
| Two body propagation | stable | 1.0 | Analytic solution of Kepler's equation |
| SGP4 / SDP4 | stable | 1.2 | Reads standard two line element sets |
| Numerical integration | beta | 2.0 | Runge-Kutta 4 and Dormand-Prince 8 |
| Atmospheric drag | beta | 2.0 | Harris-Priester density model |
| Solar radiation pressure | alpha | 2.1 | Cylindrical shadow only |
| Ground track projection | stable | 1.1 | WGS84 and spherical Earth |
| Lambert solver | stable | 1.4 | Both short and long way transfers |

## Quick start

1. Load a two line element set from a file or from the network.
2. Choose a propagator that matches the accuracy you need.
3. Step the state vector forward in time.
4. Convert the result into the reference frame you care about.
5. Render or export the resulting trajectory.

The library ships with a handful of reference frames:

- Earth centered inertial (`ECI`)
- Earth centered Earth fixed (`ECEF`)
- Geodetic latitude, longitude and altitude
- Topocentric horizon, relative to an observer
  - azimuth and elevation
  - slant range and range rate

> Reference frame conversions are the single most common source of errors in orbital
> software. Every conversion in this library is covered by tests against published
> test vectors, and the tolerances are documented in the API reference.
>
> When in doubt, prefer the explicit `ToFrame` overloads over the implicit ones.

## Propagators

A propagator advances a state vector in time. Pick one based on the trade off between
speed and accuracy that fits the mission:

### Analytic

The analytic propagator solves Kepler's equation with a Newton iteration. It is by far
the fastest option and it is exact for an ideal two body problem, but it ignores every
perturbation. Use it for quick previews, for tests and for back of the envelope work.

### SGP4

SGP4 is the model that two line element sets are published for. Using any other model
with a TLE will give you worse results, not better ones, because the element set is
fitted to the model itself. See <https://celestrak.org/> for a longer explanation and
for the current catalogue.

### Numerical

The numerical propagator integrates the equations of motion directly and lets you add
force models one by one. It is the slowest option, and the only one that can model
drag, solar radiation pressure and third body effects at the same time.

Perturbation magnitudes for a typical low Earth orbit:

| Perturbation | Acceleration (m/s²) |
| --- | --: |
| Earth J2 | 1.0e-2 |
| Atmospheric drag | 1.0e-6 |
| Third body, Moon | 5.0e-6 |
| Third body, Sun | 2.0e-6 |
| Solar radiation pressure | 9.0e-8 |

## Maneuver planning

Maneuvers are expressed as impulsive delta-v vectors in the orbital frame. The cost of
a Hohmann transfer between two circular orbits is $\Delta v = v_1 (\sqrt{2 r_2 / (r_1 +
r_2)} - 1)$, and the library exposes it directly:

```csharp
var transfer = Maneuvers.Hohmann(fromRadius: 6_878_137.0, toRadius: 42_164_000.0);
Console.WriteLine(transfer.TotalDeltaV);
```

For non coplanar transfers use the Lambert solver instead, which returns *both* the
short way and the long way solution so you can compare them. A plane change is
expensive: changing inclination by one degree in low Earth orbit costs roughly
`130 m/s`, which is why launch azimuth matters so much.

---

## Roadmap

- [x] Two body propagation
- [x] SGP4 and ground tracks
- [x] Lambert solver
- [ ] Full ~~spherical harmonics~~ gravity field up to degree 70
- [ ] Station keeping optimizer
- [ ] Constellation coverage analysis

## Contributing

Contributions are welcome :rocket: — please read the guidelines first.

1. Fork the repository and create a topic branch.
2. Add tests for the behaviour you are changing.
   1. Unit tests go next to the code they cover.
   2. Reference vector tests go into the conformance suite.
3. Run the full suite locally before pushing.
4. Open a pull request and describe *what* changed and *why*.

Please keep commits focused: a commit that fixes a bug should not also reformat a
thousand lines of unrelated code. If you are unsure whether a change is in scope, open
an issue first and ask — it is much cheaper than writing code nobody merges.

### Code style

Formatting is enforced by the analyzers, so there is nothing to argue about. The only
rules that are not automated are naming and documentation: public members need XML
documentation comments, and names should be spelled out rather than abbreviated.

See the [contributing guide](https://example.org/contributing) and the
[code of conduct](https://example.org/conduct) for the details.

## License

Released under the MIT license. See [LICENSE](https://example.org/license) for the full
text. Commercial support is available, contact sales@example.org for a quote.
