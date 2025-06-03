(define (problem problem)
	(:domain domain-elevator-normal-capacity-infinity)
	(:objects
		elevator1 - elevator
		floor1 floor2 - floor
		person1 - person
	)
	(:init
		(at-person person1 floor1)
		(at-elevator elevator1 floor1)
		(target person1 floor2)
		(above floor2 floor1)
	)
	(:goal
		(and
			(reached person1)
		)
	)
)
