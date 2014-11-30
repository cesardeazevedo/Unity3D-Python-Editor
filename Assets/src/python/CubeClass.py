import UnityEngine as unity

class CubeClass():
    
    vel = 1.5
    
    def Awake(self, this):
        unity.Debug.Log("Awake Method")

    def Start(self, this):
        unity.Debug.Log("Start Method")
        this.renderer.material.color = unity.Color.red      
    
    def Update(self, this):
        horizontal = unity.Input.GetAxis("Horizontal") * self.vel * unity.Time.deltaTime
        vertical   = unity.Input.GetAxis("Vertical")   * self.vel * unity.Time.deltaTime
        this.transform.Translate(horizontal,vertical, 0)

