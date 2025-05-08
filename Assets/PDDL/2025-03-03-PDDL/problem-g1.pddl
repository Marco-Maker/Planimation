; example in the slide of previous lecture. 2 rooms, 2 balls, 1 gripper. No types

(define (problem pb1)
    (:domain gripper)
    (:requirements :strips)
  	(:objects roomA roomB Ball1 Ball2 left)
	(:init 
		(room roomA)
		(room roomB)
		(ball Ball1)
		(ball Ball2)
		(gripper left)
		(at-robby roomA) 
		(free left) 
		(at Ball1 roomA)
		(at Ball2 roomA))
	(:goal (and (at Ball1 roomB) 
		(at Ball2 roomB)))
)
