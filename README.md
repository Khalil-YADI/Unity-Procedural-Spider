>  **[Watch the Procedural Animation Demonstration on YouTube](INSERE_TON_LIEN_YOUTUBE_ICI)**

---

### Procedural Walking Animation & Terrain Adaptation

Unlike traditional pre-baked animations, this robot relies entirely on **procedural animation** and **inverse kinematics (IK) principles** to adapt dynamically to its environment. The movement is calculated in real-time using continuous mathematical functions and vector geometry.

#### 1. Gait Cycle & Phase Management
The walk cycle is driven by a continuous time variable $t$, normalized within a $[0, 2]$ loop. To simulate a realistic quadrupedal trot, diagonal leg pairs are assigned an alternating phase offset $\phi$.

For a given leg, its local time $t_{leg}$ is defined as:

$$t_{leg} = (t + \phi) \pmod 2$$

Where $\phi = 0$ for the Front-Left and Back-Right legs, and $\phi = 1$ for the Front-Right and Back-Left legs. This ensures a perfectly synchronized alternating gait.

#### 2. Foot Trajectory Evaluation
Instead of linear interpolation, the feet follow a natural arc driven by custom `AnimationCurves`. The system calculates the local coordinate of each foot using independent horizontal ($C_h$) and vertical ($C_v$) curves. 

The local position vector $\vec{P}_{local}$ for a foot relative to its base position $\vec{P}_{base}$ is computed as:

$$\vec{P}_{local} = \vec{P}_{base} + \vec{u}_{move} \Big[ S_{dyn} \cdot (C_h(t_{leg}) - 0.5) \Big] + \vec{u}_{up} \Big[ C_v(t_{leg}) \Big]$$

* $S_{dyn}$ is the dynamic stride multiplier, smoothed over time to prevent sudden teleports during start/stop.
* The subtraction of $0.5$ centers the horizontal curve, ensuring the foot swings equally forward and backward relative to its anchor point.
* Raycasting is then projected downward from $\vec{P}_{local}$ to snap the foot onto the terrain mesh.

#### 3. Surface Alignment & Anti-Gimbal Lock (Vector Cross Products)
To prevent the robot from clipping into slopes or flipping unexpectedly, the main body must orient itself to the terrain's average normal $\vec{n}$. To avoid Gimbal Lock issues common with Euler angles, the script constructs a pure, orthogonal rotation matrix using cross products.

1.  **New Up Vector:** Set directly to the terrain's average normal.

    $$\vec{Y}_{new} = \vec{n}$$

2.  **Forward Movement Projection:** The robot's intended movement direction ($\vec{X}_{old}$) is projected onto the new ground plane to ensure velocity is never lost to downward slopes.
```math
\vec{X}_{new} = \frac{\vec{X}_{old} - (\vec{X}_{old} \cdot \vec{Y}_{new})\vec{Y}_{new}}{\|\vec{X}_{old} - (\vec{X}_{old} \cdot \vec{Y}_{new})\vec{Y}_{new}\|}
```

3.  **Orthogonal Z-Axis Calculation:** Using the right-hand rule, the final axis $\vec{Z}_{new}$ is derived via the cross product of the adjusted X and Y axes.
```math
\vec{Z}_{new} = \frac{\vec{X}_{new} \times \vec{Y}_{new}}{\|\vec{X}_{new} \times \vec{Y}_{new}\|}
```

By feeding $\vec{Z}_{new}$ and $\vec{Y}_{new}$ into a Quaternion LookRotation, we generate a highly stable, mathematically robust orientation frame that smoothly interpolates (via `Lerp`) as the robot traverses uneven terrain.