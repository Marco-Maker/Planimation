(define (problem problem)
	(:domain domain-elevator-normal-capacity-infinity)
	(:objects
		elevator1 elevator2 - elevator
		floor1 floor2 floor3 - floor
		person1 person2 - person
	)
	(:init
		(at-person person2 floor1)
		(at-person person1 floor3)
		(target person1 floor3)
		(target person2 floor3)
		(above floor2 floor1)
		(above floor3 floor2)
		(at-elevator elevator1 floor1)
	)
	(:goal
		(and
			(reached person1)
			(reached person2)
		)
	)
)
