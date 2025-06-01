(define (problem problem)
	(:domain domain-robot-normal)
	(:objects
		room1 room2 room3 - room
		obj1 - obj
		robot1 - robot
	)
	(:init
		(at-robot robot1 room1)
		(at-obj obj1 room2)
		(free robot1)
		(allowed robot1 room1)
		(allowed robot1 room2)
		(allowed robot1 room3)
		(connected room1 room2)
		(connected room1 room3)
	)
	(:goal
		(and
			(at-obj obj1 room3)
		)
	)
)
