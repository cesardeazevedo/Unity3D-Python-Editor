import UnityEngine as unity

class CubeClass():
    
    vel = 1.5

    def Update(self, this):
        horizontal = unity.Input.GetAxis("Horizontal") * self.vel * unity.Time.deltaTime
        vertical   = unity.Input.GetAxis("Vertical")   * self.vel * unity.Time.deltaTime
        this.transform.Translate(horizontal, 0, vertical)
