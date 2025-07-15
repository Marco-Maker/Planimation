(define (problem p)
	(:domain domain-robot-temporal)
	(:objects
		room1 room2 - room
		obj1 - obj
		robot1 - robot
	)
	(:init
		(at-robot robot1 room1)
		(at-obj obj1 room1)
		(free robot1)
		(allowed robot1 room1)
		(allowed robot1 room2)
		(= (move-time robot1) 1)
		(= (battery robot1) 21)
	)
	(:goal
		(and
			(at-obj obj1 room2)
		)
	)
)
