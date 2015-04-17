import UnityEngine as u

class RigidbodyExample():

    rigid = None

    def Awake(self, this):
        self.rigid = this.GetComponent(u.Rigidbody)      

    def Update(self, this):
 
        if(u.Input.GetKeyDown(u.KeyCode.Space)):
        	self.rigid.velocity = u.Vector3(0, 5, 0)

