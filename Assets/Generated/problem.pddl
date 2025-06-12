(define (problem p)
	(:domain domain-robot-event)
	(:objects
		garden1 garden2 - garden
		obj1 - obj
		robot1 - robot
	)
	(:init
		(at-robot robot1 garden1)
		(at-obj obj1 garden1)
		(free robot1)
		(allowed robot1 garden1)
		(allowed robot1 garden2)
		(path garden1 garden2)
		(= (battery robot1) 25)
		(= (distance garden1 garden2) 30)
		(= (distance garden2 garden1) 30)
		(= (speed robot1) 1)
	)
	(:goal
		(and
			(at-obj obj1 garden2)
		)
	)
)
