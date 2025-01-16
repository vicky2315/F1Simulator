For my Masters Project, F1 Simulator was created to replicate the competitive AI racing agents in AAA games. It was implemented in Unity and the agents were trained with the help of MLAgents framework. 

Game Design : 
1) Cars :

Two sets of cars, one with steady acceleration and no suspension or drag and the second car was made to replicate a F1 car with features such as mass distribution, suspension and drag. A DRS system was implemented for both of the cars which detected cars ahead within 1 second in designated DRS zones and reduced drag coefficient to increase top speed for hard difficulty car, adds extra speed 
for easy difficulty car.

2) Tracks 

The 2 tracks that were developed in the game are Monza Circuit and the Redbull Ring Circuit.

3) Lap Timer System

A lap timer system has been implemented that records lap times of each car and compares it with the 
previous lap times, returning a positive/negative reward for the car if it has performed better.

4) Race Manager

  The Race Manager stores the progress of all cars, maintaining real-time updates on their positions 
relative to checkpoints. The RaceManager is made a Singleton to make sure that only one instance of 
the class exists throughout the game.

5) Reward Functions 

Positive Rewards: 
o Maintaining optimal racing lines using the alignment of the car with the checkpoints. 
o Minimizing lap times. 
o Successfully overtaking opponents. 
o Utilisation of DRS. 

Negative Rewards: 
o Collisions with track boundaries or other cars. 
o Recording a time 2 seconds more than the previous lap time. 
o Dropping positions.

Learning Methods

Training was performed using two different car types based on their difficulty levels. Reinforcement 
learning was effective for the easy car whereas a combination of RL, GAIL and behavioural cloning was 
used to train the difficult car.
