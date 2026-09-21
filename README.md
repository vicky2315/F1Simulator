# F1 Simulator 

For my Masters Project, F1 Simulator was created to replicate the competitive AI racing agents in AAA games. It was implemented in Unity and the agents were trained with the help of MLAgents framework. 

## Learning Methods

Training was performed using two different car types based on their difficulty levels. Reinforcement 
learning was effective for the easy car whereas a combination of RL, GAIL and behavioural cloning was 
used to train the difficult car.

## Game Design : 
### 1) Cars :

Two sets of cars, one with steady acceleration and no suspension or drag and the second car was made to replicate a F1 car with features such as mass distribution, suspension and drag. A DRS system was implemented for both of the cars which detected cars ahead within 1 second in designated DRS zones and reduced drag coefficient to increase top speed for hard difficulty car, adds extra speed 
for easy difficulty car.

### 2) Tracks 

The 2 tracks that were developed in the game are Monza Circuit and the Redbull Ring Circuit.

### 3) Lap Timer System

A lap timer system has been implemented that records lap times of each car and compares it with the 
previous lap times, returning a positive/negative reward for the car if it has performed better.

### 4) Race Manager

  The Race Manager stores the progress of all cars, maintaining real-time updates on their positions 
relative to checkpoints. The RaceManager is made a Singleton to make sure that only one instance of 
the class exists throughout the game.

### 5) Reward Functions 

Positive Rewards: 
o Maintaining optimal racing lines using the alignment of the car with the checkpoints. 
o Minimizing lap times. 
o Successfully overtaking opponents. 
o Utilisation of DRS. 

Negative Rewards: 
o Collisions with track boundaries or other cars. 
o Recording a time 2 seconds more than the previous lap time. 
o Dropping positions.

## How It Actually Works

Both cars are ML-Agents `Agent` subclasses — `CarController` for the easy car, `CarControllerImproved`
for the hard car (adds `WheelCollider`s for real suspension/mass/drag). Each takes two discrete
actions — throttle (back/none/forward) and steer (left/none/right) — and gets a `Heuristic()` fallback
so you can drive it by hand for testing:

```csharp
switch (actions.DiscreteActions.Array[0])
{
    case 1: rb.AddRelativeForce(Vector3.back * Movespeed * Time.deltaTime, ForceMode.VelocityChange);
            AddReward(multback); break;
    case 2: rb.AddRelativeForce(Vector3.forward * (Movespeed / 2) * Time.deltaTime, ForceMode.VelocityChange);
            AddReward(multfwd); break;
}
```

Rewards come straight off collisions and checkpoints — hit the wrong checkpoint and it's a penalty, hit
a wall and it's a penalty, cross the finish line and you get a lap-completion reward plus a bonus if
that lap beat your best:

```csharp
if (directionDot > 0) { AddReward(0.5f); }              // correct checkpoint
else { AddReward(-5.0f); checkPointList.Remove(other.gameObject); }  // wrong checkpoint

if (other.gameObject.tag == "Final") { AddReward(5.0f); AddReward(BetterLapReward); EndEpisode(); }
```

`LapTimer` decides that lap bonus by comparing against the car's own best time:

```csharp
if (lapTime <= bestLapTime) { bestLapTime = lapTime; isBetter = 3.0f; }
else if (lapTime - bestLapTime < 1.5f) { isBetter = 0.0f; }
else { isBetter = -3.0f; }
```

DRS is a raycast check, not a scripted zone trigger — three rays (center/left/right) look for a car
ahead within range while inside a DRS zone, and only then apply the speed boost:

```csharp
if (Physics.Raycast(rayStartCenter, transform.forward, out hit, drsRange) && hit.transform.CompareTag("Car"))
    carAhead = hit.transform;
```

`RaceManager` is a singleton tracking every car's position and lap time so the leaderboard and reward
functions (like the position-based ones above) all read from one shared source of truth.

## Author
Vignesh Suresh — [portfolio](https://vicky2315.github.io)

