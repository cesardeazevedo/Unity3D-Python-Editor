import UnityEngine as u

class Player(): 

    def Start(self, this):
        pass

    def Update(self, this):
    	this.transform.Rotate(u.Vector3(20,20,10))
    	this.transform.position = u.Vector3(u.Mathf.Sin(u.Time.time), 0, u.Mathf.Cos(u.Time.time))
