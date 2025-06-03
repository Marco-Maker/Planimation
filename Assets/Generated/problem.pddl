(define (problem problem)
	(:domain domain-robot-normal)
	(:objects
<<<<<<< Updated upstream
		room1 room2 - room
=======
		room1 room2 room3 room4 room5 room6 room7 - room
>>>>>>> Stashed changes
		obj1 - obj
		robot1 - robot
	)
	(:init
		(at-robot robot1 room1)
		(at-obj obj1 room1)
		(free robot1)
		(carry obj1 robot1)
		(allowed robot1 room1)
<<<<<<< Updated upstream
		(connected room1 room2)
	)
	(:goal
		(and
			(at-obj obj1 room1)
=======
		(allowed robot1 room6)
		(connected room1 room2)
		(connected room1 room3)
		(connected room1 room4)
		(connected room1 room5)
		(connected room1 room6)
		(connected room1 room7)
	)
	(:goal
		(and
			(at-obj obj1 room6)
>>>>>>> Stashed changes
		)
	)
)
